using APPLICATION_LAYER.DTOs.Chat;
using APPLICATION_LAYER.Services.Interfaces;
using AutoMapper;
using DOMAIN_LAYER.Repository;
using Serilog;

namespace APPLICATION_LAYER.Services.Implementations
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ChatService(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ChatMessageResponseDto> SendMessageAsync(SendMessageDto dto, int userId)
        {
            try
            {
                _logger.Information($"Sending message to trip: {dto.TripId} from user: {userId}");

                // Check if trip exists
                var trip = await _unitOfWork.Trips.GetByIdAsync(dto.TripId);
                if (trip == null)
                {
                    _logger.Warning($"Trip not found: {dto.TripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                // Check if user is a member of the trip
                var isUserMember = trip.HostId == userId || (await _unitOfWork.TripMembers.GetMembershipsByUserAsync(userId))
                    .Any(m => m.TripId == dto.TripId);
                if (!isUserMember)
                {
                    _logger.Warning($"User {userId} is not a member of trip {dto.TripId}");
                    throw new InvalidOperationException("User is not a member of this trip");
                }

                // Create chat message
                var message = new DOMAIN_LAYER.Entity.ChatMessage.ChatMessage
                {
                    TripId = dto.TripId,
                    SenderId = userId,
                    Message = dto.Message,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ChatMessages.AddAsync(message);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"Message sent successfully with ID: {message.Id}");

                // Map to response DTO with sender name
                var sender = await _unitOfWork.Users.GetByIdAsync(userId);
                var responseDto = _mapper.Map<ChatMessageResponseDto>(message);
                responseDto.SenderName = sender?.Name ?? "";

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending message: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ChatMessageResponseDto>> GetMessagesByTripAsync(int tripId)
        {
            try
            {
                _logger.Information($"Fetching messages for trip: {tripId}");

                // Verify trip exists
                var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
                if (trip == null)
                {
                    _logger.Warning($"Trip not found: {tripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                var messages = await _unitOfWork.ChatMessages.GetMessagesByTripAsync(tripId);

                var responseDtos = new List<ChatMessageResponseDto>();
                foreach (var message in messages)
                {
                    var sender = await _unitOfWork.Users.GetByIdAsync(message.SenderId);
                    var responseDto = _mapper.Map<ChatMessageResponseDto>(message);
                    responseDto.SenderName = sender?.Name ?? "";
                    responseDtos.Add(responseDto);
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching messages for trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ChatMessageResponseDto>> GetMessagesByTripPaginatedAsync(int tripId, int pageNumber, int pageSize)
        {
            try
            {
                _logger.Information($"Fetching paginated messages for trip: {tripId}, page: {pageNumber}, size: {pageSize}");

                // Verify trip exists
                var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
                if (trip == null)
                {
                    _logger.Warning($"Trip not found: {tripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                var messages = await _unitOfWork.ChatMessages.GetMessagesByTripPaginatedAsync(tripId, pageNumber, pageSize);

                var responseDtos = new List<ChatMessageResponseDto>();
                foreach (var message in messages)
                {
                    var sender = await _unitOfWork.Users.GetByIdAsync(message.SenderId);
                    var responseDto = _mapper.Map<ChatMessageResponseDto>(message);
                    responseDto.SenderName = sender?.Name ?? "";
                    responseDtos.Add(responseDto);
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching paginated messages for trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ChatMessageResponseDto>> GetLatestMessagesAsync(int tripId, int count)
        {
            try
            {
                _logger.Information($"Fetching latest {count} messages for trip: {tripId}");

                // Verify trip exists
                var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
                if (trip == null)
                {
                    _logger.Warning($"Trip not found: {tripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                var messages = await _unitOfWork.ChatMessages.GetLatestMessagesAsync(tripId, count);

                var responseDtos = new List<ChatMessageResponseDto>();
                foreach (var message in messages)
                {
                    var sender = await _unitOfWork.Users.GetByIdAsync(message.SenderId);
                    var responseDto = _mapper.Map<ChatMessageResponseDto>(message);
                    responseDto.SenderName = sender?.Name ?? "";
                    responseDtos.Add(responseDto);
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching latest messages for trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ChatMessageResponseDto>> GetMessagesBySenderAsync(int userId)
        {
            try
            {
                _logger.Information($"Fetching messages sent by user: {userId}");
                var messages = await _unitOfWork.ChatMessages.GetMessagesBySenderAsync(userId);

                var responseDtos = new List<ChatMessageResponseDto>();
                foreach (var message in messages)
                {
                    var sender = await _unitOfWork.Users.GetByIdAsync(message.SenderId);
                    var responseDto = _mapper.Map<ChatMessageResponseDto>(message);
                    responseDto.SenderName = sender?.Name ?? "";
                    responseDtos.Add(responseDto);
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching messages sent by user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ChatMessageResponseDto>> SearchMessagesAsync(int tripId, string searchText)
        {
            try
            {
                _logger.Information($"Searching messages in trip: {tripId} with text: {searchText}");

                // Verify trip exists
                var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
                if (trip == null)
                {
                    _logger.Warning($"Trip not found: {tripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                var messages = await _unitOfWork.ChatMessages.SearchMessagesAsync(tripId, searchText);

                var responseDtos = new List<ChatMessageResponseDto>();
                foreach (var message in messages)
                {
                    var sender = await _unitOfWork.Users.GetByIdAsync(message.SenderId);
                    var responseDto = _mapper.Map<ChatMessageResponseDto>(message);
                    responseDto.SenderName = sender?.Name ?? "";
                    responseDtos.Add(responseDto);
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error searching messages in trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetMessageCountAsync(int tripId)
        {
            try
            {
                _logger.Information($"Getting message count for trip: {tripId}");
                var count = await _unitOfWork.ChatMessages.GetMessageCountAsync(tripId);
                return count;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting message count for trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteMessageAsync(int messageId, int userId)
        {
            try
            {
                _logger.Information($"Deleting message: {messageId} by user: {userId}");

                var message = await _unitOfWork.ChatMessages.GetByIdAsync(messageId);
                if (message == null)
                {
                    _logger.Warning($"Message not found: {messageId}");
                    throw new InvalidOperationException("Message not found");
                }

                // Verify user is the sender
                if (message.SenderId != userId)
                {
                    _logger.Warning($"User {userId} is not the sender of message {messageId}");
                    throw new InvalidOperationException("Only the sender can delete the message");
                }

                await _unitOfWork.ChatMessages.DeleteAsync(message.Id);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"Message deleted successfully: {messageId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting message {messageId}: {ex.Message}");
                throw;
            }
        }
    }
}
