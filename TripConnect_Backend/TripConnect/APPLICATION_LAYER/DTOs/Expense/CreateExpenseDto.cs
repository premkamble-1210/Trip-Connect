namespace APPLICATION_LAYER.DTOs.Expense
{
    /// <summary>
    /// Create Expense DTO
    /// </summary>
    public class CreateExpenseDto
    {
        public int TripId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public List<ExpenseSplitItemDto> Splits { get; set; } // Users to split with
    }

    /// <summary>
    /// Expense Split Item DTO
    /// </summary>
    public class ExpenseSplitItemDto
    {
        public int UserId { get; set; }
        public decimal AmountOwed { get; set; }
    }
}
