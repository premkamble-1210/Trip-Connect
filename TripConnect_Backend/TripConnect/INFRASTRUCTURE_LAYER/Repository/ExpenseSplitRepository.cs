using DOMAIN_LAYER.Repository;
using DOMAIN_LAYER.Entity.ExpenseSplit;
using Microsoft.EntityFrameworkCore;  // ✅ Correct

namespace INFRASTRUCTURE_LAYER.Repository
{
    /// <summary>
    /// ExpenseSplit Repository Implementation - Specific operations for ExpenseSplit entity
    /// </summary>
    public class ExpenseSplitRepository : Repository<ExpenseSplit>, IExpenseSplitRepository
    {
        public ExpenseSplitRepository(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get splits by expense
        /// </summary>
        public async Task<IEnumerable<ExpenseSplit>> GetSplitsByExpenseAsync(int expenseId)
        {
            return await _dbSet.Where(es => es.ExpenseId == expenseId).ToListAsync();
        }

        /// <summary>
        /// Get splits owed by user in a trip
        /// </summary>
        public async Task<IEnumerable<ExpenseSplit>> GetOwedSplitsByUserAsync(int userId, int tripId)
        {
            return await _dbSet.Include(es => es.Expense)
                               .Where(es => es.UserId == userId && es.Expense.TripId == tripId)
                               .ToListAsync();
        }

        /// <summary>
        /// Get unsettled splits for user
        /// </summary>
        public async Task<IEnumerable<ExpenseSplit>> GetUnsettledSplitsByUserAsync(int userId, int tripId)
        {
            return await _dbSet.Include(es => es.Expense)
                               .Where(es => es.UserId == userId && es.Expense.TripId == tripId && !es.IsSettled)
                               .ToListAsync();
        }

        /// <summary>
        /// Get total amount owed by user in a trip
        /// </summary>
        public async Task<decimal> GetTotalOwedByUserAsync(int userId, int tripId)
        {
            return await _dbSet.Include(es => es.Expense)
                               .Where(es => es.UserId == userId && es.Expense.TripId == tripId && !es.IsSettled)
                               .SumAsync(es => es.AmountOwed);
        }

        /// <summary>
        /// Mark split as settled
        /// </summary>
        public async Task MarkAsSettledAsync(int splitId)
        {
            var split = await GetByIdAsync(splitId);
            if (split != null)
            {
                split.IsSettled = true;
                await UpdateAsync(split);
            }
        }

        /// <summary>
        /// Get settled status of split
        /// </summary>
        public async Task<bool> IsSettledAsync(int splitId)
        {
            var split = await GetByIdAsync(splitId);
            return split?.IsSettled ?? false;
        }

        /// <summary>
        /// Get all unsettled splits for a trip
        /// </summary>
        public async Task<IEnumerable<ExpenseSplit>> GetAllUnsettledSplitsAsync(int tripId)
        {
            return await _dbSet.Include(es => es.Expense)
                               .Where(es => es.Expense.TripId == tripId && !es.IsSettled)
                               .ToListAsync();
        }
    }
}
