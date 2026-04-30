using APPLICATION_LAYER.DTOs.Trip;
using APPLICATION_LAYER.DTOs.TripMember;
using APPLICATION_LAYER.Services.Interfaces;
using AutoMapper;
using DOMAIN_LAYER.Entity.Trip;
using DOMAIN_LAYER.Enum;
using DOMAIN_LAYER.Repository;
using INFRASTRUCTURE_LAYER.Cache;
using Serilog;

namespace APPLICATION_LAYER.Services.Implementations
{
    public class TripService : ITripService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public TripService(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<TripResponseDto> CreateTripAsync(CreateTripDto createTripDto, int userId)
        {
            try
            {
                _logger.Information($"Creating new trip for user: {userId}");

                var newTrip = _mapper.Map<Trip>(createTripDto);
                newTrip.HostId = userId;
                newTrip.Status = TripStatus.Planned;
                newTrip.CreatedAt = DateTime.UtcNow;

                if (createTripDto.TripDays?.Any() == true)
                    newTrip.TripDays = _mapper.Map<List<TripDay>>(createTripDto.TripDays);

                await _unitOfWork.Trips.AddAsync(newTrip);
                await _unitOfWork.SaveChangesAsync();

                // Invalidate trip list caches
                await _cacheService.InvalidateByTagAsync("trip:all");
                await _cacheService.InvalidateByTagAsync($"trip:user:{userId}");

                _logger.Information($"Trip created successfully with ID: {newTrip.Id}");

                return _mapper.Map<TripResponseDto>(newTrip);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating trip: {ex.Message}");
                throw;
            }
        }

        public async Task<TripResponseDto> GetTripByIdAsync(int tripId)
        {
            try
            {
                _logger.Information($"Fetching trip: {tripId}");
                
                // Try to get from cache first
                var cacheKey = string.Format(CacheKeyConstants.TRIP_BY_ID, tripId);
                var trip = await _cacheService.GetOrSetAsync(
                    cacheKey,
                    async () => await _unitOfWork.Trips.GetByIdAsync(tripId),
                    TimeSpan.FromHours(1)
                );

                if (trip == null)
                {
                    _logger.Warning($"Trip not found: {tripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                // Add tags to the cache entry
                await _cacheService.SetAsync(cacheKey, trip, TimeSpan.FromHours(1), new[] { $"trip:{tripId}", "trip:all" });

                return _mapper.Map<TripResponseDto>(trip);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<TripResponseDto>> GetAllTripsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.Information($"Fetching all trips - Page: {pageNumber}, Size: {pageSize}");
                
                // Create cache key that includes pagination parameters
                var cacheKey = $"trip:all:page:{pageNumber}:size:{pageSize}";
                
                var cachedTrips = await _cacheService.GetAsync<IEnumerable<TripResponseDto>>(cacheKey);
                if (cachedTrips != null)
                {
                    _logger.Information($"Trips cache hit - Page: {pageNumber}, Size: {pageSize}");
                    return cachedTrips;
                }

                // Cache miss - fetch from database
                var trips = await _unitOfWork.Trips.GetAllAsync();

                // Apply pagination
                var paginatedTrips = trips
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(paginatedTrips);

                // Cache the paginated result with 1 hour TTL
                await _cacheService.SetAsync(
                    cacheKey,
                    tripDtos,
                    TimeSpan.FromHours(1),
                    new[] { "trip:all", "trip:list" }
                );

                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching all trips: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<TripResponseDto>> GetTripsByStatusAsync(string status)
        {
            try
            {
                _logger.Information($"Fetching trips by status: {status}");

                if (!Enum.TryParse<TripStatus>(status, true, out var tripStatus))
                {
                    throw new InvalidOperationException($"Invalid status: {status}");
                }

                // Create cache key that includes status parameter
                var cacheKey = $"trip:status:{status.ToLower()}";
                
                var cachedTrips = await _cacheService.GetAsync<IEnumerable<TripResponseDto>>(cacheKey);
                if (cachedTrips != null)
                {
                    _logger.Information($"Trips status cache hit: {status}");
                    return cachedTrips;
                }

                var trips = await _unitOfWork.Trips.GetTripsByStatusAsync(tripStatus);
                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(trips);

                // Cache the result with 1 hour TTL
                await _cacheService.SetAsync(
                    cacheKey,
                    tripDtos,
                    TimeSpan.FromHours(1),
                    new[] { "trip:all", "trip:list" }
                );

                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching trips by status {status}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<TripResponseDto>> GetUpcomingTripsAsync()
        {
            try
            {
                _logger.Information("Fetching upcoming trips");
                
                var cacheKey = "trip:upcoming";
                
                var cachedTrips = await _cacheService.GetAsync<IEnumerable<TripResponseDto>>(cacheKey);
                if (cachedTrips != null)
                {
                    _logger.Information("Upcoming trips cache hit");
                    return cachedTrips;
                }

                var trips = await _unitOfWork.Trips.GetUpcomingTripsAsync();
                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(trips);

                // Cache the result with 30 minutes TTL (upcoming trips change frequently)
                await _cacheService.SetAsync(
                    cacheKey,
                    tripDtos,
                    TimeSpan.FromMinutes(30),
                    new[] { "trip:all", "trip:upcoming" }
                );

                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching upcoming trips: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<TripResponseDto>> SearchTripsByLocationAsync(string location)
        {
            try
            {
                _logger.Information($"Searching trips by location: {location}");
                
                // Create cache key that includes location parameter
                var cacheKey = $"trip:location:{location.ToLower()}";
                
                var cachedTrips = await _cacheService.GetAsync<IEnumerable<TripResponseDto>>(cacheKey);
                if (cachedTrips != null)
                {
                    _logger.Information($"Trips location cache hit: {location}");
                    return cachedTrips;
                }

                var trips = await _unitOfWork.Trips.GetTripsByLocationAsync(location);
                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(trips);

                // Cache the result with 2 hours TTL
                await _cacheService.SetAsync(
                    cacheKey,
                    tripDtos,
                    TimeSpan.FromHours(2),
                    new[] { "trip:all", "trip:search", $"trip:location:{location.ToLower()}" }
                );

                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error searching trips by location {location}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<TripResponseDto>> SearchTripsAsync(string location, DateTime? startDate, decimal? maxBudget, string travelType)
        {
            try
            {
                _logger.Information($"Searching trips with criteria - Location: {location}, StartDate: {startDate}, MaxBudget: {maxBudget}, TravelType: {travelType}");

                // Create cache key that includes all search parameters
                var cacheKey = $"trip:search:{location?.ToLower()}:{startDate?.Date}:{maxBudget}:{travelType?.ToLower()}";
                
                var cachedTrips = await _cacheService.GetAsync<IEnumerable<TripResponseDto>>(cacheKey);
                if (cachedTrips != null)
                {
                    _logger.Information($"Trips search cache hit");
                    return cachedTrips;
                }

                var trips = await _unitOfWork.Trips.SearchTripsAsync(location, startDate, maxBudget, travelType);
                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(trips);

                // Cache the result with 2 hours TTL
                await _cacheService.SetAsync(
                    cacheKey,
                    tripDtos,
                    TimeSpan.FromHours(2),
                    new[] { "trip:all", "trip:search" }
                );

                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error searching trips: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<TripResponseDto>> GetTripsCreatedByUserAsync(int userId)
        {
            try
            {
                _logger.Information($"Fetching trips created by user: {userId}");
                
                // Create cache key that includes userId
                var cacheKey = $"trip:user:{userId}:created";
                
                var cachedTrips = await _cacheService.GetAsync<IEnumerable<TripResponseDto>>(cacheKey);
                if (cachedTrips != null)
                {
                    _logger.Information($"User trips cache hit: {userId}");
                    return cachedTrips;
                }

                var trips = await _unitOfWork.Trips.GetTripsByHostAsync(userId);
                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(trips);

                // Cache the result with 1 hour TTL
                await _cacheService.SetAsync(
                    cacheKey,
                    tripDtos,
                    TimeSpan.FromHours(1),
                    new[] { $"trip:user:{userId}", "trip:all" }
                );

                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching trips created by user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<TripResponseDto> UpdateTripAsync(int tripId, UpdateTripDto updateTripDto, int userId)
        {
            try
            {
                _logger.Information($"Updating trip: {tripId}");
                var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);

                if (trip == null)
                {
                    _logger.Warning($"Trip not found for update: {tripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                // Verify user is the trip host
                if (trip.HostId != userId)
                {
                    _logger.Warning($"User {userId} is not the host of trip {tripId}");
                    throw new InvalidOperationException("Only the trip host can update the trip");
                }

                // Map DTO to trip entity
                _mapper.Map(updateTripDto, trip);

                // Replace TripDays if provided
                if (updateTripDto.TripDays != null)
                {
                    trip.TripDays.Clear();
                    foreach (var dayDto in updateTripDto.TripDays)
                    {
                        var day = _mapper.Map<TripDay>(dayDto);
                        day.TripId = trip.Id;
                        trip.TripDays.Add(day);
                    }
                }

                await _unitOfWork.Trips.UpdateAsync(trip);
                await _unitOfWork.SaveChangesAsync();

                // Invalidate cache
                var cacheKey = string.Format(CacheKeyConstants.TRIP_BY_ID, tripId);
                await _cacheService.InvalidateByTagAsync($"trip:{tripId}");
                await _cacheService.InvalidateByTagAsync("trip:all");
                await _cacheService.RemoveAsync(cacheKey);

                _logger.Information($"Trip updated successfully and cache invalidated: {tripId}");
                return _mapper.Map<TripResponseDto>(trip);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CancelTripAsync(int tripId, int userId)
        {
            try
            {
                _logger.Information($"Cancelling trip: {tripId}");
                var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);

                if (trip == null)
                {
                    _logger.Warning($"Trip not found for cancellation: {tripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                // Verify user is the trip host
                if (trip.HostId != userId)
                {
                    _logger.Warning($"User {userId} is not the host of trip {tripId}");
                    throw new InvalidOperationException("Only the trip host can cancel the trip");
                }

                trip.Status = TripStatus.Cancelled;

                await _unitOfWork.Trips.UpdateAsync(trip);
                await _unitOfWork.SaveChangesAsync();

                // Invalidate cache
                await _cacheService.InvalidateByTagAsync($"trip:{tripId}");
                await _cacheService.InvalidateByTagAsync("trip:all");

                _logger.Information($"Trip cancelled successfully and cache invalidated: {tripId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error cancelling trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<TripResponseDto>> GetUserTripsAsync(int userId)
        {
            try
            {
                _logger.Information($"Fetching trips for user: {userId}");

                // Create cache key that includes userId
                var cacheKey = $"trip:user:{userId}:member";
                
                var cachedTrips = await _cacheService.GetAsync<IEnumerable<TripResponseDto>>(cacheKey);
                if (cachedTrips != null)
                {
                    _logger.Information($"User member trips cache hit: {userId}");
                    return cachedTrips;
                }

                // Get trips where user is a member
                var tripMembers = await _unitOfWork.TripMembers.GetMembershipsByUserAsync(userId);
                var tripIds = tripMembers.Select(tm => tm.TripId).ToList();

                if (!tripIds.Any())
                {
                    // Cache empty result too
                    var emptyTrips = new List<TripResponseDto>();
                    await _cacheService.SetAsync(
                        cacheKey,
                        emptyTrips,
                        TimeSpan.FromHours(1),
                        new[] { $"trip:user:{userId}" }
                    );
                    return emptyTrips;
                }

                var trips = new List<Trip>();
                foreach (var tripId in tripIds)
                {
                    var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
                    if (trip != null)
                    {
                        trips.Add(trip);
                    }
                }

                var tripDtos = _mapper.Map<IEnumerable<TripResponseDto>>(trips);

                // Cache the result with 1 hour TTL
                await _cacheService.SetAsync(
                    cacheKey,
                    tripDtos,
                    TimeSpan.FromHours(1),
                    new[] { $"trip:user:{userId}", "trip:all" }
                );

                return tripDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching trips for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<TripMemberResponseDto>> GetTripMembersAsync(int tripId)
        {
            try
            {
                _logger.Information($"Fetching members for trip: {tripId}");

                var tripMembers = await _unitOfWork.TripMembers.GetMembersByTripAsync(tripId);
                var memberDtos = new List<TripMemberResponseDto>();

                foreach (var member in tripMembers)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(member.UserId);
                    memberDtos.Add(new TripMemberResponseDto
                    {
                        Id = member.Id,
                        UserId = member.UserId,
                        UserName = user?.Name ?? "Unknown",
                        Role = member.Role.ToString(),
                        JoinedAt = member.JoinedAt
                    });
                }

                return memberDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching members for trip {tripId}: {ex.Message}");
                throw;
            }
        }
    }
}
