using APPLICATION_LAYER.DTOs.Expense;

namespace APPLICATION_LAYER.Services.Interfaces
{
    /// <summary>
    /// Expense Service Interface - Handles expense tracking and splitting
    /// </summary>
    public interface IExpenseService
    {
        /// <summary>
        /// Create expense and split among members
        /// </summary>
        Task<ExpenseResponseDto> CreateExpenseAsync(CreateExpenseDto dto, int userId);

        /// <summary>
        /// Get expense by ID
        /// </summary>
        Task<ExpenseResponseDto> GetExpenseByIdAsync(int expenseId);

        /// <summary>
        /// Get all expenses for a trip
        /// </summary>
        Task<IEnumerable<ExpenseResponseDto>> GetExpensesByTripAsync(int tripId);

        /// <summary>
        /// Get expenses paid by user
        /// </summary>
        Task<IEnumerable<ExpenseResponseDto>> GetExpensesPaidByUserAsync(int userId);

        /// <summary>
        /// Get total expenses for trip
        /// </summary>
        Task<decimal> GetTotalExpensesByTripAsync(int tripId);

        /// <summary>
        /// Get expenses owed by user in trip
        /// </summary>
        Task<decimal> GetTotalOwedByUserAsync(int userId, int tripId);

        /// <summary>
        /// Get unsettled expenses for user in trip
        /// </summary>
        Task<IEnumerable<ExpenseResponseDto>> GetUnsettledExpensesByUserAsync(int userId, int tripId);

        /// <summary>
        /// Mark expense split as settled
        /// </summary>
        Task<bool> SettleExpenseAsync(int splitId);

        /// <summary>
        /// Delete expense
        /// </summary>
        Task<bool> DeleteExpenseAsync(int expenseId, int userId);

        /// <summary>
        /// Get expense summary for trip
        /// </summary>
        Task<ExpenseSummaryDto> GetExpenseSummaryAsync(int tripId);
    }

    /// <summary>
    /// Expense summary DTO
    /// </summary>
    public class ExpenseSummaryDto
    {
        public decimal TotalExpenses { get; set; }
        public List<UserOwesDto> UserBalances { get; set; }
    }

    /// <summary>
    /// User owes DTO for settlement
    /// </summary>
    public class UserOwesDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal AmountOwed { get; set; }
    }
}
