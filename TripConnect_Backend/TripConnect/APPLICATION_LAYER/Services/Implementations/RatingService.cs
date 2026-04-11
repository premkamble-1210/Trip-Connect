using APPLICATION_LAYER.DTOs.Rating;
using APPLICATION_LAYER.Services.Interfaces;
using AutoMapper;
using DOMAIN_LAYER.Repository;
using Serilog;

namespace APPLICATION_LAYER.Services.Implementations
{
    public class RatingService : IRatingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public RatingService(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<RatingResponseDto> CreateRatingAsync(CreateRatingDto dto, int userId)
        {
            try
            {
                _logger.Information($"Creating rating for user: {dto.RatedUserId} by user: {userId} in trip: {dto.TripId}");

                // Check if trip exists
                var trip = await _unitOfWork.Trips.GetByIdAsync(dto.TripId);
                if (trip == null)
                {
                    _logger.Warning($"Trip not found: {dto.TripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                // Check if user cannot rate themselves
                if (dto.RatedUserId == userId)
                {
                    _logger.Warning($"User {userId} cannot rate themselves");
                    throw new InvalidOperationException("You cannot rate yourself");
                }

                // Check if rated user exists
                var ratedUser = await _unitOfWork.Users.GetByIdAsync(dto.RatedUserId);
                if (ratedUser == null)
                {
                    _logger.Warning($"Rated user not found: {dto.RatedUserId}");
                    throw new InvalidOperationException("Rated user not found");
                }

                // Check if user already rated the person in this trip
                var existingRating = await _unitOfWork.TripRatings.HasUserRatedAsync(dto.TripId, userId, dto.RatedUserId);
                if (existingRating)
                {
                    _logger.Warning($"User {userId} already rated {dto.RatedUserId} in trip {dto.TripId}");
                    throw new InvalidOperationException("You have already rated this user in this trip");
                }

                // Validate rating score
                if (dto.Rating < 1.0 || dto.Rating > 5.0)
                {
                    _logger.Warning($"Invalid rating score: {dto.Rating}");
                    throw new InvalidOperationException("Rating must be between 1.0 and 5.0");
                }

                // Create rating
                var rating = new DOMAIN_LAYER.Entity.TripRating.TripRating
                {
                    TripId = dto.TripId,
                    RatedBy = userId,
                    RatedUserId = dto.RatedUserId,
                    Rating = dto.Rating,
                    Review = dto.Review ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.TripRatings.AddAsync(rating);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"Rating created successfully with ID: {rating.Id}");

                // Map to response DTO
                var responseDto = _mapper.Map<RatingResponseDto>(rating);
                responseDto.RatedByName = (await _unitOfWork.Users.GetByIdAsync(rating.RatedBy))?.Name ?? "";
                responseDto.RatedUserName = ratedUser.Name;

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating rating: {ex.Message}");
                throw;
            }
        }

        public async Task<RatingResponseDto> GetRatingByIdAsync(int ratingId)
        {
            try
            {
                _logger.Information($"Fetching rating: {ratingId}");
                var rating = await _unitOfWork.TripRatings.GetByIdAsync(ratingId);

                if (rating == null)
                {
                    _logger.Warning($"Rating not found: {ratingId}");
                    throw new InvalidOperationException("Rating not found");
                }

                var responseDto = _mapper.Map<RatingResponseDto>(rating);
                responseDto.RatedByName = (await _unitOfWork.Users.GetByIdAsync(rating.RatedBy))?.Name ?? "";
                responseDto.RatedUserName = (await _unitOfWork.Users.GetByIdAsync(rating.RatedUserId))?.Name ?? "";

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching rating {ratingId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<RatingResponseDto>> GetRatingsForUserAsync(int userId)
        {
            try
            {
                _logger.Information($"Fetching ratings for user: {userId}");
                var ratings = await _unitOfWork.TripRatings.GetRatingsReceivedByUserAsync(userId);

                var responseDtos = new List<RatingResponseDto>();
                foreach (var rating in ratings)
                {
                    var responseDto = _mapper.Map<RatingResponseDto>(rating);
                    responseDto.RatedByName = (await _unitOfWork.Users.GetByIdAsync(rating.RatedBy))?.Name ?? "";
                    responseDto.RatedUserName = (await _unitOfWork.Users.GetByIdAsync(rating.RatedUserId))?.Name ?? "";
                    responseDtos.Add(responseDto);
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching ratings for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<double> GetAverageRatingAsync(int userId)
        {
            try
            {
                _logger.Information($"Calculating average rating for user: {userId}");
                var average = await _unitOfWork.TripRatings.GetAverageRatingForUserAsync(userId);
                return average;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error calculating average rating for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<RatingResponseDto>> GetRatingsGivenByUserAsync(int userId)
        {
            try
            {
                _logger.Information($"Fetching ratings given by user: {userId}");
                var ratings = await _unitOfWork.TripRatings.GetRatingsGivenByUserAsync(userId);

                var responseDtos = new List<RatingResponseDto>();
                foreach (var rating in ratings)
                {
                    var responseDto = _mapper.Map<RatingResponseDto>(rating);
                    responseDto.RatedByName = (await _unitOfWork.Users.GetByIdAsync(rating.RatedBy))?.Name ?? "";
                    responseDto.RatedUserName = (await _unitOfWork.Users.GetByIdAsync(rating.RatedUserId))?.Name ?? "";
                    responseDtos.Add(responseDto);
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching ratings given by user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<RatingResponseDto>> GetRatingsForTripAsync(int tripId)
        {
            try
            {
                _logger.Information($"Fetching ratings for trip: {tripId}");

                // Verify trip exists
                var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
                if (trip == null)
                {
                    _logger.Warning($"Trip not found: {tripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                var ratings = await _unitOfWork.TripRatings.GetRatingsByTripAsync(tripId);

                var responseDtos = new List<RatingResponseDto>();
                foreach (var rating in ratings)
                {
                    var responseDto = _mapper.Map<RatingResponseDto>(rating);
                    responseDto.RatedByName = (await _unitOfWork.Users.GetByIdAsync(rating.RatedBy))?.Name ?? "";
                    responseDto.RatedUserName = (await _unitOfWork.Users.GetByIdAsync(rating.RatedUserId))?.Name ?? "";
                    responseDtos.Add(responseDto);
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching ratings for trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> HasUserRatedAsync(int tripId, int ratedBy, int ratedUserId)
        {
            try
            {
                _logger.Information($"Checking if user {ratedBy} rated {ratedUserId} in trip {tripId}");
                var hasRated = await _unitOfWork.TripRatings.HasUserRatedAsync(tripId, ratedBy, ratedUserId);
                return hasRated;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error checking if user rated: {ex.Message}");
                throw;
            }
        }

        public async Task<RatingResponseDto> UpdateRatingAsync(int ratingId, CreateRatingDto dto, int userId)
        {
            try
            {
                _logger.Information($"Updating rating: {ratingId} by user: {userId}");

                var rating = await _unitOfWork.TripRatings.GetByIdAsync(ratingId);
                if (rating == null)
                {
                    _logger.Warning($"Rating not found: {ratingId}");
                    throw new InvalidOperationException("Rating not found");
                }

                // Verify user is the one who gave the rating
                if (rating.RatedBy != userId)
                {
                    _logger.Warning($"User {userId} did not give rating {ratingId}");
                    throw new InvalidOperationException("Only the person who gave the rating can update it");
                }

                // Validate rating score
                if (dto.Rating < 1.0 || dto.Rating > 5.0)
                {
                    _logger.Warning($"Invalid rating score: {dto.Rating}");
                    throw new InvalidOperationException("Rating must be between 1.0 and 5.0");
                }

                // Update rating
                rating.Rating = dto.Rating;
                rating.Review = dto.Review ?? string.Empty;

                await _unitOfWork.TripRatings.UpdateAsync(rating);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"Rating updated successfully: {ratingId}");

                // Map to response DTO
                var responseDto = _mapper.Map<RatingResponseDto>(rating);
                responseDto.RatedByName = (await _unitOfWork.Users.GetByIdAsync(rating.RatedBy))?.Name ?? "";
                responseDto.RatedUserName = (await _unitOfWork.Users.GetByIdAsync(rating.RatedUserId))?.Name ?? "";

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating rating {ratingId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteRatingAsync(int ratingId, int userId)
        {
            try
            {
                _logger.Information($"Deleting rating: {ratingId} by user: {userId}");

                var rating = await _unitOfWork.TripRatings.GetByIdAsync(ratingId);
                if (rating == null)
                {
                    _logger.Warning($"Rating not found: {ratingId}");
                    throw new InvalidOperationException("Rating not found");
                }

                // Verify user is the one who gave the rating
                if (rating.RatedBy != userId)
                {
                    _logger.Warning($"User {userId} did not give rating {ratingId}");
                    throw new InvalidOperationException("Only the person who gave the rating can delete it");
                }

                await _unitOfWork.TripRatings.DeleteAsync(rating.Id);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information($"Rating deleted successfully: {ratingId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting rating {ratingId}: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetRatingsCountAsync(int userId)
        {
            try
            {
                _logger.Information($"Getting ratings count for user: {userId}");
                var count = await _unitOfWork.TripRatings.GetRatingsCountAsync(userId);
                return count;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting ratings count for user {userId}: {ex.Message}");
                throw;
            }
        }
    }
}
