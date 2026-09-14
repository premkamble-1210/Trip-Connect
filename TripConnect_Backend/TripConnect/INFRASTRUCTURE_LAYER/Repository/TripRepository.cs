using DOMAIN_LAYER.Repository;
using DOMAIN_LAYER.Entity.Trip;
using DOMAIN_LAYER.Enum;
using Microsoft.EntityFrameworkCore;  // ✅ Correct

namespace INFRASTRUCTURE_LAYER.Repository
{
    /// <summary>
    /// Trip Repository Implementation - Specific operations for Trip entity
    /// </summary>
    public class TripRepository : Repository<Trip>, ITripRepository
    {
        public TripRepository(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get trip by ID with TripDays and Host eagerly loaded
        /// </summary>
        public override async Task<Trip> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(t => t.TripDays)
                .Include(t => t.Host)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Get trips by status
        /// </summary>
        public async Task<IEnumerable<Trip>> GetTripsByStatusAsync(TripStatus status)
        {
            return await _dbSet.Where(t => t.Status == status).ToListAsync();
        }

        /// <summary>
        /// Get trips by location
        /// </summary>
        public async Task<IEnumerable<Trip>> GetTripsByLocationAsync(string location)
        {
            return await _dbSet.Where(t => t.Location.Contains(location)).ToListAsync();
        }

        /// <summary>
        /// Get trips by host/organizer
        /// </summary>
        public async Task<IEnumerable<Trip>> GetTripsByHostAsync(int hostId)
        {
            return await _dbSet.Where(t => t.HostId == hostId).ToListAsync();
        }

        /// <summary>
        /// Get upcoming trips (not started yet)
        /// </summary>
        public async Task<IEnumerable<Trip>> GetUpcomingTripsAsync()
        {
            return await _dbSet.Where(t => t.Status == TripStatus.Planned && t.StartDate > DateTime.Now)
                               .OrderBy(t => t.StartDate)
                               .ToListAsync();
        }

        /// <summary>
        /// Get trips within budget range
        /// </summary>
        public async Task<IEnumerable<Trip>> GetTripsByBudgetAsync(decimal minBudget, decimal maxBudget)
        {
            return await _dbSet.Where(t => t.Budget >= minBudget && t.Budget <= maxBudget).ToListAsync();
        }

        /// <summary>
        /// Get trips by travel type
        /// </summary>
        public async Task<IEnumerable<Trip>> GetTripsByTravelTypeAsync(string travelType)
        {
            return await _dbSet.Where(t => t.TravelType == travelType).ToListAsync();
        }

        /// <summary>
        /// Search trips by multiple criteria
        /// </summary>
        public async Task<IEnumerable<Trip>> SearchTripsAsync(string location, DateTime? startDate, decimal? minBudget, decimal? maxBudget, string travelType, DateTime? endDate)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(location))
                query = query.Where(t => t.Location.Contains(location));

            if (startDate.HasValue)
                query = query.Where(t => t.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.EndDate <= endDate.Value);

            if (minBudget.HasValue)
                query = query.Where(t => t.Budget >= minBudget.Value);

            if (maxBudget.HasValue)
                query = query.Where(t => t.Budget <= maxBudget.Value);

            if (!string.IsNullOrEmpty(travelType))
                query = query.Where(t => t.TravelType == travelType);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get the ranked discover feed.
        /// Filters to upcoming trips with open seats and orders by a weighted score
        /// (popularity, host trust, freshness, availability).
        /// </summary>
        public async Task<(IEnumerable<Trip> Trips, int TotalCount)> GetDiscoverTripsAsync(int pageNumber, int pageSize)
        {
            var today = DateTime.Now.Date;

            var candidates = await _dbSet
                .Where(t => t.Status == TripStatus.Planned && t.EndDate >= today && t.Host != null)
                .Select(t => new
                {
                    t.Id,
                    t.Seats,
                    t.StartDate,
                    FilledSeats = t.TripMembers.Count(tm => tm.Status == TripMemberStatus.Active),
                    AcceptedRequests = t.TripRequests.Count(tr => tr.Status == TripRequestStatus.Accepted),
                    HostRating = t.Host.Rating,
                    DaysSinceCreated = EF.Functions.DateDiffDay(t.CreatedAt, today)
                })
                .ToListAsync();

            var maxAccepted = candidates.Count > 0 ? candidates.Max(c => c.AcceptedRequests) : 0;

            var ranked = candidates
                .Where(c => c.FilledSeats < c.Seats)
                .Select(c => new
                {
                    c.Id,
                    c.Seats,
                    c.FilledSeats,
                    Score = ScoreTrip(c.AcceptedRequests, maxAccepted, c.HostRating, c.DaysSinceCreated, c.Seats, c.FilledSeats)
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Id)
                .ToList();

            var totalCount = ranked.Count;

            var pageIds = ranked
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.Id)
                .ToList();

            if (pageIds.Count == 0)
                return (new List<Trip>(), totalCount);

            var pageTrips = await _dbSet
                .Include(t => t.Host)
                .Include(t => t.TripMembers)
                .Where(t => pageIds.Contains(t.Id))
                .ToListAsync();

            // Preserve the ranking order (EF doesn't guarantee order from the id list)
            var orderedTrips = pageIds
                .Select(id => pageTrips.First(t => t.Id == id))
                .ToList();

            return (orderedTrips, totalCount);
        }

        /// <summary>
        /// Gets trips the user has interacted with: trips they host, active memberships,
        /// or trips for which they have a pending/accepted join request.
        /// </summary>
        public async Task<IEnumerable<Trip>> GetUserHistoryTripsAsync(int userId)
        {
            return await _dbSet
                .Where(t => t.HostId == userId ||
                            t.TripMembers.Any(m => m.UserId == userId && m.Status == TripMemberStatus.Active) ||
                            t.TripRequests.Any(r => r.UserId == userId &&
                                (r.Status == TripRequestStatus.Pending || r.Status == TripRequestStatus.Accepted)))
                .ToListAsync();
        }

        /// <summary>
        /// Top ranked discover candidates for a user, excluding the user's own trips and
        /// trips they already joined or requested. Result is loaded with Host and TripMembers
        /// for DTO mapping, already ordered by the Phase-A quality score.
        /// </summary>
        public async Task<IEnumerable<Trip>> GetRecommendationCandidatesAsync(int userId, int take)
        {
            var today = DateTime.Now.Date;

            var candidates = await _dbSet
                .Where(t => t.Status == TripStatus.Planned &&
                            t.EndDate >= today &&
                            t.Host != null &&
                            t.HostId != userId &&
                            !t.TripMembers.Any(m => m.UserId == userId && m.Status == TripMemberStatus.Active) &&
                            !t.TripRequests.Any(r => r.UserId == userId &&
                                (r.Status == TripRequestStatus.Pending || r.Status == TripRequestStatus.Accepted)))
                .Select(t => new
                {
                    t.Id,
                    t.Seats,
                    t.StartDate,
                    FilledSeats = t.TripMembers.Count(tm => tm.Status == TripMemberStatus.Active),
                    AcceptedRequests = t.TripRequests.Count(tr => tr.Status == TripRequestStatus.Accepted),
                    HostRating = t.Host.Rating,
                    DaysSinceCreated = EF.Functions.DateDiffDay(t.CreatedAt, today)
                })
                .ToListAsync();

            var maxAccepted = candidates.Count > 0 ? candidates.Max(c => c.AcceptedRequests) : 0;

            var rankedIds = candidates
                .Where(c => c.FilledSeats < c.Seats)
                .Select(c => new
                {
                    c.Id,
                    Score = ScoreTrip(c.AcceptedRequests, maxAccepted, c.HostRating, c.DaysSinceCreated, c.Seats, c.FilledSeats)
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Id)
                .Take(take)
                .Select(x => x.Id)
                .ToList();

            if (rankedIds.Count == 0)
                return new List<Trip>();

            var pool = await _dbSet
                .Include(t => t.Host)
                .Include(t => t.TripMembers)
                .Where(t => rankedIds.Contains(t.Id))
                .ToListAsync();

            return rankedIds
                .Select(id => pool.First(t => t.Id == id))
                .ToList();
        }

        /// <summary>
        /// Weighted score: 45% popularity, 25% host trust, 20% freshness, 10% availability.
        /// </summary>
        private static double ScoreTrip(int acceptedRequests, int maxAccepted, double hostRating, int daysSinceCreated, int seats, int filledSeats)
        {
            double popularity = maxAccepted > 0 ? Math.Log(1 + acceptedRequests) / Math.Log(1 + maxAccepted) : 0;
            double hostTrust = Math.Min(hostRating, 5.0) / 5.0;
            double freshness = Math.Exp(-(double)daysSinceCreated / 30.0);
            double availability = seats > 0 ? (double)(seats - filledSeats) / seats : 0;

            return (0.45 * popularity) + (0.25 * hostTrust) + (0.20 * freshness) + (0.10 * availability);
        }

        /// <summary>
        /// Get trip with all related data (includes members, requests, expenses)
        /// </summary>
        public async Task<Trip> GetTripWithDetailsAsync(int tripId)
        {
            return await _dbSet.Include(t => t.TripMembers)
                               .Include(t => t.TripRequests)
                               .Include(t => t.Expenses)
                               .Include(t => t.Ratings)
                               .Include(t => t.TripDays)
                               .FirstOrDefaultAsync(t => t.Id == tripId);
        }
    }
}
