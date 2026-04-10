namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// ExpenseSplit Repository Interface - Specific operations for ExpenseSplit entity
    /// </summary>
    public interface IExpenseSplitRepository : IRepository<Entity.ExpenseSplit.ExpenseSplit>
    {
        /// <summary>
        /// Get splits by expense
        /// </summary>
        Task<IEnumerable<Entity.ExpenseSplit.ExpenseSplit>> GetSplitsByExpenseAsync(int expenseId);

        /// <summary>
        /// Get splits owed by user in a trip
        /// </summary>
        Task<IEnumerable<Entity.ExpenseSplit.ExpenseSplit>> GetOwedSplitsByUserAsync(int userId, int tripId);

        /// <summary>
        /// Get unsettled splits for user
        /// </summary>
        Task<IEnumerable<Entity.ExpenseSplit.ExpenseSplit>> GetUnsettledSplitsByUserAsync(int userId, int tripId);

        /// <summary>
        /// Get total amount owed by user in a trip
        /// </summary>
        Task<decimal> GetTotalOwedByUserAsync(int userId, int tripId);

        /// <summary>
        /// Mark split as settled
        /// </summary>
        Task MarkAsSettledAsync(int splitId);

        /// <summary>
        /// Get settled status of split
        /// </summary>
        Task<bool> IsSettledAsync(int splitId);

        /// <summary>
        /// Get all unsettled splits for a trip
        /// </summary>
        Task<IEnumerable<Entity.ExpenseSplit.ExpenseSplit>> GetAllUnsettledSplitsAsync(int tripId);
    }
}
