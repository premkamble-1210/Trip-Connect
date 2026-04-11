using DOMAIN_LAYER.Repository;
using DOMAIN_LAYER.Entity.Expense;
using Microsoft.EntityFrameworkCore;  // ✅ Correct

namespace INFRASTRUCTURE_LAYER.Repository
{
    /// <summary>
    /// Expense Repository Implementation - Specific operations for Expense entity
    /// </summary>
    public class ExpenseRepository : Repository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get expenses by trip
        /// </summary>
        public async Task<IEnumerable<Expense>> GetExpensesByTripAsync(int tripId)
        {
            return await _dbSet.Where(e => e.TripId == tripId).ToListAsync();
        }

        /// <summary>
        /// Get expenses paid by user
        /// </summary>
        public async Task<IEnumerable<Expense>> GetExpensesByUserAsync(int userId)
        {
            return await _dbSet.Where(e => e.PaidBy == userId).ToListAsync();
        }

        /// <summary>
        /// Get total expenses for trip
        /// </summary>
        public async Task<decimal> GetTotalExpensesByTripAsync(int tripId)
        {
            return await _dbSet.Where(e => e.TripId == tripId).SumAsync(e => e.Amount);
        }

        /// <summary>
        /// Get total expenses paid by user in a trip
        /// </summary>
        public async Task<decimal> GetTotalPaidByUserInTripAsync(int userId, int tripId)
        {
            return await _dbSet.Where(e => e.TripId == tripId && e.PaidBy == userId).SumAsync(e => e.Amount);
        }

        /// <summary>
        /// Get expenses by category/description
        /// </summary>
        public async Task<IEnumerable<Expense>> GetExpensesByDescriptionAsync(int tripId, string description)
        {
            return await _dbSet.Where(e => e.TripId == tripId && e.Description == description).ToListAsync();
        }

        /// <summary>
        /// Get expense with splits
        /// </summary>
        public async Task<Expense> GetExpenseWithSplitsAsync(int expenseId)
        {
            return await _dbSet.Include(e => e.ExpenseSplits)
                               .FirstOrDefaultAsync(e => e.Id == expenseId);
        }

        /// <summary>
        /// Delete expense with related splits
        /// </summary>
        public async Task DeleteExpenseWithSplitsAsync(int expenseId)
        {
            var expense = await GetExpenseWithSplitsAsync(expenseId);
            if (expense != null)
            {
                // Note: Splits will be deleted automatically if cascade delete is configured
                await DeleteAsync(expenseId);
            }
        }
    }
}
