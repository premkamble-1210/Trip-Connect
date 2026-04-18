using APPLICATION_LAYER.DTOs.Trip;
using APPLICATION_LAYER.DTOs.TripMember;
using APPLICATION_LAYER.Services.Interfaces;
using AutoMapper;
using DOMAIN_LAYER.Entity.Trip;
using DOMAIN_LAYER.Enum;
using DOMAIN_LAYER.Repository;
using Serilog;

namespace APPLICATION_LAYER.Services.Implementations
{
    public class TripService : ITripService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public TripService(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
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

                await _unitOfWork.Trips.AddAsync(newTrip);
                await _unitOfWork.SaveChangesAsync();

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
                var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);

                if (trip == null)
                {
                    _logger.Warning($"Trip not found: {tripId}");
                    throw new InvalidOperationException("Trip not found");
                }

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
                var trips = await _unitOfWork.Trips.GetAllAsync();

                // Apply pagination
                var paginatedTrips = trips
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                return _mapper.Map<IEnumerable<TripResponseDto>>(paginatedTrips);
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

                var trips = await _unitOfWork.Trips.GetTripsByStatusAsync(tripStatus);
                return _mapper.Map<IEnumerable<TripResponseDto>>(trips);
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
                var trips = await _unitOfWork.Trips.GetUpcomingTripsAsync();
                return _mapper.Map<IEnumerable<TripResponseDto>>(trips);
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
                var trips = await _unitOfWork.Trips.GetTripsByLocationAsync(location);
                return _mapper.Map<IEnumerable<TripResponseDto>>(trips);
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

                var trips = await _unitOfWork.Trips.SearchTripsAsync(location, startDate, maxBudget, travelType);
                return _mapper.Map<IEnumerable<TripResponseDto>>(trips);
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
                var trips = await _unitOfWork.Trips.GetTripsByHostAsync(userId);
                return _mapper.Map<IEnumerable<TripResponseDto>>(trips);
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

                await _unitOfWork.Trips.UpdateAsync(trip);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"Trip updated successfully: {tripId}");
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

                _logger.Information($"Trip cancelled successfully: {tripId}");
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

                // Get trips where user is a member
                var tripMembers = await _unitOfWork.TripMembers.GetMembershipsByUserAsync(userId);
                var tripIds = tripMembers.Select(tm => tm.TripId).ToList();

                if (!tripIds.Any())
                {
                    return new List<TripResponseDto>();
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

                return _mapper.Map<IEnumerable<TripResponseDto>>(trips);
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
