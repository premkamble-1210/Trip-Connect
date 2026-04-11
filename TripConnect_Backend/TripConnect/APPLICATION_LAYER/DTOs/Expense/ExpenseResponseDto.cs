namespace APPLICATION_LAYER.DTOs.Expense
{
    /// <summary>
    /// Expense Response DTO
    /// </summary>
    public class ExpenseResponseDto
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int PaidBy { get; set; }
        public string PaidByName { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ExpenseSplitResponseDto> Splits { get; set; }
    }

    /// <summary>
    /// Expense Split Response DTO
    /// </summary>
    public class ExpenseSplitResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal AmountOwed { get; set; }
        public bool IsSettled { get; set; }
    }
}
