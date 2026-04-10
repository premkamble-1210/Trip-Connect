namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// Expense Repository Interface - Specific operations for Expense entity
    /// </summary>
    public interface IExpenseRepository : IRepository<Entity.Expense.Expense>
    {
        /// <summary>
        /// Get expenses by trip
        /// </summary>
        Task<IEnumerable<Entity.Expense.Expense>> GetExpensesByTripAsync(int tripId);

        /// <summary>
        /// Get expenses paid by user
        /// </summary>
        Task<IEnumerable<Entity.Expense.Expense>> GetExpensesByUserAsync(int userId);

        /// <summary>
        /// Get total expenses for trip
        /// </summary>
        Task<decimal> GetTotalExpensesByTripAsync(int tripId);

        /// <summary>
        /// Get total expenses paid by user in a trip
        /// </summary>
        Task<decimal> GetTotalPaidByUserInTripAsync(int userId, int tripId);

        /// <summary>
        /// Get expenses by category/description
        /// </summary>
        Task<IEnumerable<Entity.Expense.Expense>> GetExpensesByDescriptionAsync(int tripId, string description);

        /// <summary>
        /// Get expense with splits
        /// </summary>
        Task<Entity.Expense.Expense> GetExpenseWithSplitsAsync(int expenseId);

        /// <summary>
        /// Delete expense with related splits
        /// </summary>
        Task DeleteExpenseWithSplitsAsync(int expenseId);
    }
}
