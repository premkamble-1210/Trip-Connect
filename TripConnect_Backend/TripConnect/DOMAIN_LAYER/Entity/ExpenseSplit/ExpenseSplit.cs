namespace DOMAIN_LAYER.Entity.ExpenseSplit
{
    /// <summary>
    /// ExpenseSplit Entity - Represents how an expense is split among trip members
    /// </summary>
    public class ExpenseSplit
    {
        /// <summary>
        /// Unique identifier for the expense split
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Expense ID this split belongs to
        /// </summary>
        public int ExpenseId { get; set; }

        /// <summary>
        /// User ID of the person who owes money
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Amount owed by this user for this expense
        /// </summary>
        public decimal AmountOwed { get; set; }

        /// <summary>
        /// Flag indicating if this portion of the expense has been settled
        /// </summary>
        public bool IsSettled { get; set; }

        /// <summary>
        /// Navigation property - Expense this split belongs to (1:M)
        /// </summary>
        public DOMAIN_LAYER.Entity.Expense.Expense Expense { get; set; }

        /// <summary>
        /// Navigation property - User who owes this split (1:M)
        /// </summary>
        public DOMAIN_LAYER.Entity.User.User User { get; set; }
    }
}
