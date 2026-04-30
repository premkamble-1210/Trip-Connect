using APPLICATION_LAYER.DTOs.Expense;
using APPLICATION_LAYER.Services.Interfaces;
using AutoMapper;
using DOMAIN_LAYER.Entity.ExpenseSplit;
using DOMAIN_LAYER.Repository;
using DOMAIN_LAYER.Enum;
using INFRASTRUCTURE_LAYER.Cache;
using Serilog;

namespace APPLICATION_LAYER.Services.Implementations
{
    public class ExpenseService : IExpenseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public ExpenseService(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<ExpenseResponseDto> CreateExpenseAsync(CreateExpenseDto dto, int userId)
        {
            try
            {
                _logger.Information($"Creating expense for trip: {dto.TripId} by user: {userId}");

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

                // Create expense
                var expense = new DOMAIN_LAYER.Entity.Expense.Expense
                {
                    TripId = dto.TripId,
                    PaidBy = userId,
                    Amount = dto.Amount,
                    Description = dto.Description,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Expenses.AddAsync(expense);
                await _unitOfWork.SaveChangesAsync();

                // Invalidate trip expense caches
                await _cacheService.InvalidateByTagAsync($"trip:{dto.TripId}:expenses");
                await _cacheService.InvalidateByTagAsync($"user:{userId}:expenses");

                _logger.Information($"Expense created successfully with ID: {expense.Id}");

                // Map to response DTO
                var paidByUser = await _unitOfWork.Users.GetByIdAsync(userId);
                var responseDto = _mapper.Map<ExpenseResponseDto>(expense);
                responseDto.PaidByName = paidByUser?.Name ?? "";

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating expense: {ex.Message}");
                throw;
            }
        }

        public async Task<ExpenseResponseDto> GetExpenseByIdAsync(int expenseId)
        {
            try
            {
                _logger.Information($"Fetching expense: {expenseId}");
                
                var cacheKey = string.Format(CacheKeyConstants.EXPENSE_BY_ID, expenseId);
                var expense = await _cacheService.GetOrSetAsync(
                    cacheKey,
                    async () => await _unitOfWork.Expenses.GetExpenseWithSplitsAsync(expenseId),
                    TimeSpan.FromHours(1)
                );

                if (expense == null)
                {
                    _logger.Warning($"Expense not found: {expenseId}");
                    throw new InvalidOperationException("Expense not found");
                }

                // Add tags to the cache entry
                await _cacheService.SetAsync(cacheKey, expense, TimeSpan.FromHours(1), new[] { $"expense:{expenseId}" });

                var paidByUser = await _unitOfWork.Users.GetByIdAsync(expense.PaidBy);
                var responseDto = _mapper.Map<ExpenseResponseDto>(expense);
                responseDto.PaidByName = paidByUser?.Name ?? "";

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching expense {expenseId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ExpenseResponseDto>> GetExpensesByTripAsync(int tripId)
        {
            try
            {
                _logger.Information($"Fetching expenses for trip: {tripId}");

                // Verify trip exists
                var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
                if (trip == null)
                {
                    _logger.Warning($"Trip not found: {tripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                var expenses = await _unitOfWork.Expenses.GetExpensesByTripAsync(tripId);

                var responseDtos = new List<ExpenseResponseDto>();
                foreach (var expense in expenses)
                {
                    var paidByUser = await _unitOfWork.Users.GetByIdAsync(expense.PaidBy);
                    var responseDto = _mapper.Map<ExpenseResponseDto>(expense);
                    responseDto.PaidByName = paidByUser?.Name ?? "";
                    responseDtos.Add(responseDto);
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching expenses for trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ExpenseResponseDto>> GetExpensesPaidByUserAsync(int userId)
        {
            try
            {
                _logger.Information($"Fetching expenses paid by user: {userId}");
                var expenses = await _unitOfWork.Expenses.GetExpensesByUserAsync(userId);

                var responseDtos = new List<ExpenseResponseDto>();
                foreach (var expense in expenses)
                {
                    var paidByUser = await _unitOfWork.Users.GetByIdAsync(expense.PaidBy);
                    var responseDto = _mapper.Map<ExpenseResponseDto>(expense);
                    responseDto.PaidByName = paidByUser?.Name ?? "";
                    responseDtos.Add(responseDto);
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching expenses paid by user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<decimal> GetTotalExpensesByTripAsync(int tripId)
        {
            try
            {
                _logger.Information($"Calculating total expenses for trip: {tripId}");
                var total = await _unitOfWork.Expenses.GetTotalExpensesByTripAsync(tripId);
                return total;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error calculating total expenses for trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<decimal> GetTotalOwedByUserAsync(int userId, int tripId)
        {
            try
            {
                _logger.Information($"Calculating total owed by user: {userId} in trip: {tripId}");
                var total = await _unitOfWork.ExpenseSplits.GetTotalOwedByUserAsync(userId, tripId);
                return total;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error calculating total owed by user {userId} in trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ExpenseResponseDto>> GetUnsettledExpensesByUserAsync(int userId, int tripId)
        {
            try
            {
                _logger.Information($"Fetching unsettled expenses for user: {userId} in trip: {tripId}");

                var unsettledSplits = await _unitOfWork.ExpenseSplits.GetUnsettledSplitsByUserAsync(userId, tripId);
                var expenseIds = unsettledSplits.Select(s => s.ExpenseId).Distinct();

                var responseDtos = new List<ExpenseResponseDto>();
                foreach (var expenseId in expenseIds)
                {
                    var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId);
                    if (expense != null)
                    {
                        var paidByUser = await _unitOfWork.Users.GetByIdAsync(expense.PaidBy);
                        var responseDto = _mapper.Map<ExpenseResponseDto>(expense);
                        responseDto.PaidByName = paidByUser?.Name ?? "";
                        responseDtos.Add(responseDto);
                    }
                }

                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching unsettled expenses for user {userId} in trip {tripId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SettleExpenseAsync(int splitId)
        {
            try
            {
                _logger.Information($"Settling expense split: {splitId}");

                var split = await _unitOfWork.ExpenseSplits.GetByIdAsync(splitId);
                if (split == null)
                {
                    _logger.Warning($"Expense split not found: {splitId}");
                    throw new InvalidOperationException("Expense split not found");
                }

                await _unitOfWork.ExpenseSplits.MarkAsSettledAsync(splitId);
                await _unitOfWork.SaveChangesAsync();

                // Invalidate cache
                await _cacheService.InvalidateByTagAsync($"expense:{split.ExpenseId}");

                _logger.Information($"Expense split settled successfully and cache invalidated: {splitId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error settling expense split {splitId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteExpenseAsync(int expenseId, int userId)
        {
            try
            {
                _logger.Information($"Deleting expense: {expenseId} by user: {userId}");

                var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId);
                if (expense == null)
                {
                    _logger.Warning($"Expense not found: {expenseId}");
                    throw new InvalidOperationException("Expense not found");
                }

                // Verify user is the one who paid the expense
                if (expense.PaidBy != userId)
                {
                    _logger.Warning($"User {userId} did not pay expense {expenseId}");
                    throw new InvalidOperationException("Only the person who paid the expense can delete it");
                }

                await _unitOfWork.Expenses.DeleteExpenseWithSplitsAsync(expenseId);
                await _unitOfWork.SaveChangesAsync();

                // Invalidate cache
                var cacheKey = string.Format(CacheKeyConstants.EXPENSE_BY_ID, expenseId);
                await _cacheService.InvalidateByTagAsync($"expense:{expenseId}");
                await _cacheService.InvalidateByTagAsync($"trip:{expense.TripId}:expenses");
                await _cacheService.RemoveAsync(cacheKey);

                _logger.Information($"Expense deleted successfully and cache invalidated: {expenseId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting expense {expenseId}: {ex.Message}");
                throw;
            }
        }

        public async Task<ExpenseSummaryDto> GetExpenseSummaryAsync(int tripId)
        {
            try
            {
                _logger.Information($"Generating expense summary for trip: {tripId}");

                // Verify trip exists
                var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
                if (trip == null)
                {
                    _logger.Warning($"Trip not found: {tripId}");
                    throw new InvalidOperationException("Trip not found");
                }

                var totalExpenses = await _unitOfWork.Expenses.GetTotalExpensesByTripAsync(tripId);

                // Get all unsettled splits for the trip
                var unsettledSplits = await _unitOfWork.ExpenseSplits.GetAllUnsettledSplitsAsync(tripId);

                // Group by user and sum amounts owed
                var userBalances = new Dictionary<int, (decimal amount, string userName)>();
                foreach (var split in unsettledSplits)
                {
                    if (!userBalances.ContainsKey(split.UserId))
                    {
                        var user = await _unitOfWork.Users.GetByIdAsync(split.UserId);
                        userBalances[split.UserId] = (split.AmountOwed, user?.Name ?? "");
                    }
                    else
                    {
                        var current = userBalances[split.UserId];
                        userBalances[split.UserId] = (current.amount + split.AmountOwed, current.userName);
                    }
                }

                var summary = new ExpenseSummaryDto
                {
                    TotalExpenses = totalExpenses,
                    UserBalances = userBalances.Select(kvp => new UserOwesDto
                    {
                        UserId = kvp.Key,
                        UserName = kvp.Value.userName,
                        AmountOwed = kvp.Value.amount
                    }).ToList()
                };

                return summary;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error generating expense summary for trip {tripId}: {ex.Message}");
                throw;
            }
        }
    }
}
