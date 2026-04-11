using APPLICATION_LAYER.DTOs.JoinRequest;
using APPLICATION_LAYER.Services.Interfaces;
using AutoMapper;
using DOMAIN_LAYER.Entity.TripMember;
using DOMAIN_LAYER.Entity.TripRequest;
using DOMAIN_LAYER.Enum;
using DOMAIN_LAYER.Repository;
using Serilog;

namespace APPLICATION_LAYER.Services.Implementations
{
    public class JoinRequestService : IJoinRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public JoinRequestService(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<JoinRequestResponseDto> SendJoinRequestAsync(SendJoinRequestDto sendJoinRequestDto, int userId)
        {
            try
            {
                _logger.Information($"Sending join request from user: {userId} to trip: {sendJoinRequestDto.TripId}");

                // Check if trip exists
                var trip = await _unitOfWork.Trips.GetByIdAsync(sendJoinRequestDto.TripId);
                if (trip == null)
                {
                    _logger.Warning($"Trip not found: {sendJoinRequestDto.TripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                // Check if user is not the trip host
                if (trip.HostId == userId)
                {
                    _logger.Warning($"User {userId} is the trip host and cannot send join request");
                    throw new InvalidOperationException("Trip host cannot send join request");
                }

                // Check if user already requested to join
                var existingRequest = await _unitOfWork.TripRequests.GetRequestByUserAndTripAsync(userId, sendJoinRequestDto.TripId);
                if (existingRequest != null && existingRequest.Status != TripRequestStatus.Rejected && existingRequest.Status != TripRequestStatus.Cancelled)
                {
                    _logger.Warning($"User {userId} already has a pending/accepted request for trip {sendJoinRequestDto.TripId}");
                    throw new InvalidOperationException("You have already sent a request for this trip");
                }

                // Create new join request
                var newRequest = _mapper.Map<TripRequest>(sendJoinRequestDto);
                newRequest.UserId = userId;
                newRequest.Status = TripRequestStatus.Pending;

                await _unitOfWork.TripRequests.AddAsync(newRequest);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"Join request sent successfully with ID: {newRequest.Id}");

                // Map to response DTO with user name from database
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                var responseDto = _mapper.Map<JoinRequestResponseDto>(newRequest);
                responseDto.UserName = user?.Name ?? "";

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending join request: {ex.Message}");
                throw;
            }
        }

        public async Task<JoinRequestResponseDto> GetJoinRequestByIdAsync(int requestId)
        {
            try
            {
                _logger.Information($"Fetching join request: {requestId}");
                var request = await _unitOfWork.TripRequests.GetByIdAsync(requestId);

                if (request == null)
                {
                    _logger.Warning($"Join request not found: {requestId}");
                    throw new InvalidOperationException("Join request not found");
                }

                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
                var responseDto = _mapper.Map<JoinRequestResponseDto>(request);
                responseDto.UserName = user?.Name ?? "";

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching join request {requestId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<JoinRequestResponseDto>> GetPendingRequestsByTripAsync(int tripId)
        {
            try
            {
                _logger.Information($"Fetching pending requests for trip: {tripId}");
                var requests = await _unitOfWork.TripRequests.GetPendingRequestsByTripAsync(tripId);

                var responseDtos = new List<JoinRequestResponseDto>();
                foreach (var request in requests)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
                    var responseDto = _mapper.Map<JoinRequestResponseDto>(request);
                    responseDto.UserName = user?.Name ?? "";
                    responseDtos.Add(responseDto);
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching pending requests for trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<JoinRequestResponseDto>> GetRequestsByUserAsync(int userId)
        {
            try
            {
                _logger.Information($"Fetching requests sent by user: {userId}");
                var requests = await _unitOfWork.TripRequests.GetRequestsByUserAsync(userId);

                var responseDtos = new List<JoinRequestResponseDto>();
                foreach (var request in requests)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
                    var responseDto = _mapper.Map<JoinRequestResponseDto>(request);
                    responseDto.UserName = user?.Name ?? "";
                    responseDtos.Add(responseDto);
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching requests for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> AcceptJoinRequestAsync(int requestId, int tripHostId)
        {
            try
            {
                _logger.Information($"Accepting join request: {requestId}");
                var request = await _unitOfWork.TripRequests.GetByIdAsync(requestId);

                if (request == null)
                {
                    _logger.Warning($"Join request not found: {requestId}");
                    throw new InvalidOperationException("Join request not found");
                }

                // Verify trip host
                var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId);
                if (trip == null || trip.HostId != tripHostId)
                {
                    _logger.Warning($"User {tripHostId} is not the host of trip {request.TripId}");
                    throw new InvalidOperationException("Only the trip host can accept requests");
                }

                // Update request status
                await _unitOfWork.TripRequests.UpdateRequestStatusAsync(requestId, TripRequestStatus.Accepted);

                // Add user as trip member
                var tripMember = new TripMember
                {
                    TripId = request.TripId,
                    UserId = request.UserId,
                    Role = TripMemberRole.Member,
                    Status = TripMemberStatus.Active,
                    JoinedAt = DateTime.UtcNow
                };

                await _unitOfWork.TripMembers.AddAsync(tripMember);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"Join request accepted successfully: {requestId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error accepting join request {requestId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> RejectJoinRequestAsync(int requestId, int tripHostId)
        {
            try
            {
                _logger.Information($"Rejecting join request: {requestId}");
                var request = await _unitOfWork.TripRequests.GetByIdAsync(requestId);

                if (request == null)
                {
                    _logger.Warning($"Join request not found: {requestId}");
                    throw new InvalidOperationException("Join request not found");
                }

                // Verify trip host
                var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId);
                if (trip == null || trip.HostId != tripHostId)
                {
                    _logger.Warning($"User {tripHostId} is not the host of trip {request.TripId}");
                    throw new InvalidOperationException("Only the trip host can reject requests");
                }

                // Update request status
                await _unitOfWork.TripRequests.UpdateRequestStatusAsync(requestId, TripRequestStatus.Rejected);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"Join request rejected successfully: {requestId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error rejecting join request {requestId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CancelJoinRequestAsync(int requestId, int userId)
        {
            try
            {
                _logger.Information($"Cancelling join request: {requestId}");
                var request = await _unitOfWork.TripRequests.GetByIdAsync(requestId);

                if (request == null)
                {
                    _logger.Warning($"Join request not found: {requestId}");
                    throw new InvalidOperationException("Join request not found");
                }

                // Verify user is the one who sent the request
                if (request.UserId != userId)
                {
                    _logger.Warning($"User {userId} is not the sender of request {requestId}");
                    throw new InvalidOperationException("Only the request sender can cancel the request");
                }

                // Update request status
                await _unitOfWork.TripRequests.UpdateRequestStatusAsync(requestId, TripRequestStatus.Cancelled);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"Join request cancelled successfully: {requestId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error cancelling join request {requestId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> HasUserRequestedAsync(int userId, int tripId)
        {
            try
            {
                var hasRequested = await _unitOfWork.TripRequests.HasUserRequestedAsync(userId, tripId);
                return hasRequested;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error checking if user {userId} requested trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<JoinRequestResponseDto>> GetAllRequestsByTripAsync(int tripId)
        {
            try
            {
                _logger.Information($"Fetching all requests for trip: {tripId}");

                // Verify trip exists
                var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
                if (trip == null)
                {
                    _logger.Warning($"Trip not found: {tripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                var requests = await _unitOfWork.TripRequests.GetRequestsByTripAsync(tripId);

                var responseDtos = new List<JoinRequestResponseDto>();
                foreach (var request in requests)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
                    var responseDto = _mapper.Map<JoinRequestResponseDto>(request);
                    responseDto.UserName = user?.Name ?? "";
                    responseDtos.Add(responseDto);
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching all requests for trip {tripId}: {ex.Message}");
                throw;
            }
        }
    }
}
