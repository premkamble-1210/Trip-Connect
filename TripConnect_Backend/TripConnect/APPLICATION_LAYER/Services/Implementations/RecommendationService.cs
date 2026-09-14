using APPLICATION_LAYER.DTOs.Trip;
using APPLICATION_LAYER.Services.Interfaces;
using AutoMapper;
using DOMAIN_LAYER.Entity.Trip;
using DOMAIN_LAYER.Repository;
using INFRASTRUCTURE_LAYER.Cache;
using Serilog;

namespace APPLICATION_LAYER.Services.Implementations
{
    public class RecommendationService : IRecommendationService
    {
        private static readonly string[] RecommendationCacheTags = { "trip:all" };

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public RecommendationService(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<TripResponseDto>> GetRecommendationsAsync(int userId, int count = 6)
        {
            try
            {
                var cacheKey = $"trip:recommendations:{userId}";
                var cached = await _cacheService.GetAsync<IEnumerable<TripResponseDto>>(cacheKey);
                if (cached != null)
                    return cached;

                var historyTrips = (await _unitOfWork.Trips.GetUserHistoryTripsAsync(userId)).ToList();
                var profile = BuildProfile(historyTrips);

                var poolSize = Math.Max(count * 3, 12);
                var candidates = (await _unitOfWork.Trips.GetRecommendationCandidatesAsync(userId, poolSize)).ToList();

                _logger.Information($"Recommendations requested for user {userId} " +
                    $"(history signals: {profile.SignalCount}, candidates: {candidates.Count})");

                var results = new List<TripResponseDto>();
                if (candidates.Count > 0)
                {
                    var total = candidates.Count;
                    var selected = candidates
                        .Select((trip, index) => new
                        {
                            Trip = trip,
                            Score = ScoreSimilarity(trip, profile, 1 - (index / (double)total))
                        })
                        .OrderByDescending(x => x.Score)
                        .ThenBy(x => x.Trip.Id)
                        .Take(count)
                        .Select(x => x.Trip)
                        .ToList();

                    results = _mapper.Map<List<TripResponseDto>>(selected);
                }

                await _cacheService.SetAsync(cacheKey, results, TimeSpan.FromMinutes(15), RecommendationCacheTags
                    .Concat(new[] { $"recs:user:{userId}" })
                    .ToArray());

                return results;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error generating recommendations for user {userId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Build an implicit preference profile from the user's historical trips.
        /// </summary>
        private static UserPreferenceProfile BuildProfile(List<Trip> historyTrips)
        {
            var profile = new UserPreferenceProfile();

            if (historyTrips.Count == 0)
                return profile;

            double budgetSum = 0;
            double leadSum = 0;
            int budgetCount = 0;
            int leadCount = 0;

            foreach (var trip in historyTrips)
            {
                if (!string.IsNullOrWhiteSpace(trip.TravelType))
                {
                    profile.TravelTypeWeights.TryGetValue(trip.TravelType, out var weight);
                    profile.TravelTypeWeights[trip.TravelType] = weight + 1;
                }

                if (!string.IsNullOrWhiteSpace(trip.Location))
                {
                    var location = NormalizeLocation(trip.Location);
                    if (location.Length > 0)
                    {
                        profile.LocationWeights.TryGetValue(location, out var weight);
                        profile.LocationWeights[location] = weight + 1;
                    }
                }

                if (trip.Budget > 0)
                {
                    budgetSum += (double)trip.Budget;
                    budgetCount++;
                }

                var leadDays = (trip.StartDate.Date - DateTime.Now.Date).TotalDays;
                if (leadDays > 0)
                {
                    leadSum += leadDays;
                    leadCount++;
                }
            }

            if (budgetCount > 0)
            {
                profile.BudgetCenter = budgetSum / budgetCount;
                profile.BudgetSpread = historyTrips.Count > 1 && historyTrips.Max(t => t.Budget) > historyTrips.Min(t => t.Budget)
                    ? (double)(historyTrips.Max(t => t.Budget) - historyTrips.Min(t => t.Budget))
                    : profile.BudgetCenter * 0.2;
                if (profile.BudgetSpread <= 0)
                    profile.BudgetSpread = profile.BudgetCenter * 0.2;
            }

            if (leadCount > 0)
                profile.PreferredLeadDays = leadSum / leadCount;

            return profile;
        }

        /// <summary>
        /// Blend profile similarity with the Phase-A quality proxy (rewarded to 0 for cold start).
        /// </summary>
        private static double ScoreSimilarity(Trip trip, UserPreferenceProfile profile, double qualityProxy)
        {
            if (profile.SignalCount == 0)
                return qualityProxy;

            var travelType = !string.IsNullOrWhiteSpace(trip.TravelType) && profile.TravelTypeWeights.ContainsKey(trip.TravelType) ? 1.0 : 0.0;
            var location = ScoreLocation(trip.Location, profile);
            var budget = ScoreBudget(trip.Budget, profile);

            var daysToStart = (trip.StartDate.Date - DateTime.Now.Date).TotalDays;
            var dateNear = Math.Exp(-Math.Abs(daysToStart - profile.PreferredLeadDays) / 30.0);

            return 0.30 * travelType + 0.25 * location + 0.15 * budget + 0.10 * dateNear + 0.20 * qualityProxy;
        }

        /// <summary>
        /// Exact location match scores 1.0, substring containment either way scores 0.6.
        /// </summary>
        private static double ScoreLocation(string location, UserPreferenceProfile profile)
        {
            if (string.IsNullOrWhiteSpace(location) || profile.LocationWeights.Count == 0)
                return 0;

            var normalized = NormalizeLocation(location);
            if (normalized.Length == 0)
                return 0;

            if (profile.LocationWeights.ContainsKey(normalized))
                return 1.0;

            foreach (var key in profile.LocationWeights.Keys)
            {
                if (normalized.Contains(key) || key.Contains(normalized))
                    return 0.6;
            }

            return 0;
        }

        /// <summary>
        /// Budget score is 1 minus the relative distance from the user's budget center.
        /// </summary>
        private static double ScoreBudget(decimal budget, UserPreferenceProfile profile)
        {
            if (profile.BudgetCenter <= 0 || profile.BudgetSpread <= 0 || budget <= 0)
                return 0.5;

            var distance = Math.Abs((double)budget - profile.BudgetCenter);
            return Math.Clamp(1 - distance / profile.BudgetSpread, 0, 1);
        }

        private static string NormalizeLocation(string location)
            => location.Trim().ToLowerInvariant();

        /// <summary>
        /// Implicit preference profile derived from a user's history trips.
        /// </summary>
        private class UserPreferenceProfile
        {
            public Dictionary<string, int> TravelTypeWeights { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            public Dictionary<string, int> LocationWeights { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            public double BudgetCenter { get; set; }

            public double BudgetSpread { get; set; }

            public double PreferredLeadDays { get; set; } = 45.0;

            public int SignalCount => TravelTypeWeights.Values.Sum() + LocationWeights.Values.Sum();
        }
    }
}