namespace DOMAIN_LAYER.Entity.Expense
{
    /// <summary>
    /// Expense Entity - Represents an expense in a trip
    /// </summary>
    public class Expense
    {
        /// <summary>
        /// Unique identifier for the expense
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Trip ID this expense belongs to
        /// </summary>
        public int TripId { get; set; }

        /// <summary>
        /// User ID of the person who paid the expense
        /// </summary>
        public int PaidBy { get; set; }

        /// <summary>
        /// Amount of the expense
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Description/category of the expense (e.g., Hotel, Fuel, Food, etc.)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Timestamp when the expense was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Navigation property - Trip this expense belongs to (1:M)
        /// </summary>
        public DOMAIN_LAYER.Entity.Trip.Trip Trip { get; set; }

        /// <summary>
        /// Navigation property - User who paid this expense (1:M)
        /// </summary>
        public DOMAIN_LAYER.Entity.User.User PaidByUser { get; set; }

        /// <summary>
        /// Navigation property - Expense splits for this expense (1:M)
        /// </summary>
        public ICollection<DOMAIN_LAYER.Entity.ExpenseSplit.ExpenseSplit> ExpenseSplits { get; set; } = new List<DOMAIN_LAYER.Entity.ExpenseSplit.ExpenseSplit>();
    }
}
