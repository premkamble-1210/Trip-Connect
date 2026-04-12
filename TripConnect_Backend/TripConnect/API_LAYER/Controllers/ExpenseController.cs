using APPLICATION_LAYER.DTOs.Expense;
using APPLICATION_LAYER.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_LAYER.Controllers
{
    /// <summary>
    /// Expense Controller - Handles expense tracking and splitting
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(IExpenseService expenseService, ILogger<ExpenseController> logger)
        {
            _expenseService = expenseService;
            _logger = logger;
        }

        /// <summary>
        /// Create expense and split among members
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto createExpenseDto, [FromQuery] int userId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _expenseService.CreateExpenseAsync(createExpenseDto, userId);
                return CreatedAtAction(nameof(GetExpenseById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating expense");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get expense by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetExpenseById([FromRoute] int id)
        {
            try
            {
                var expense = await _expenseService.GetExpenseByIdAsync(id);
                return Ok(expense);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching expense");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get all expenses for a trip
        /// </summary>
        [HttpGet("trip/{tripId}")]
        public async Task<IActionResult> GetExpensesByTrip([FromRoute] int tripId)
        {
            try
            {
                var expenses = await _expenseService.GetExpensesByTripAsync(tripId);
                return Ok(expenses);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching expenses by trip");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get expenses paid by user
        /// </summary>
        [HttpGet("user/{userId}/paid")]
        public async Task<IActionResult> GetExpensesPaidByUser([FromRoute] int userId)
        {
            try
            {
                var expenses = await _expenseService.GetExpensesPaidByUserAsync(userId);
                return Ok(expenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching expenses paid by user");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get total expenses for trip
        /// </summary>
        [HttpGet("trip/{tripId}/total")]
        public async Task<IActionResult> GetTotalExpensesByTrip([FromRoute] int tripId)
        {
            try
            {
                var total = await _expenseService.GetTotalExpensesByTripAsync(tripId);
                return Ok(new { tripId = tripId, totalExpenses = total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total expenses");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get expenses owed by user in trip
        /// </summary>
        [HttpGet("user/{userId}/owed")]
        public async Task<IActionResult> GetTotalOwedByUser([FromRoute] int userId, [FromQuery] int tripId)
        {
            try
            {
                var total = await _expenseService.GetTotalOwedByUserAsync(userId, tripId);
                return Ok(new { userId = userId, tripId = tripId, totalOwed = total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total owed");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get unsettled expenses for user in trip
        /// </summary>
        [HttpGet("user/{userId}/unsettled")]
        public async Task<IActionResult> GetUnsettledExpensesByUser([FromRoute] int userId, [FromQuery] int tripId)
        {
            try
            {
                var expenses = await _expenseService.GetUnsettledExpensesByUserAsync(userId, tripId);
                return Ok(expenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unsettled expenses");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Mark expense split as settled
        /// </summary>
        [HttpPost("split/{splitId}/settle")]
        public async Task<IActionResult> SettleExpense([FromRoute] int splitId)
        {
            try
            {
                var result = await _expenseService.SettleExpenseAsync(splitId);
                return Ok(new { success = result, message = result ? "Expense settled successfully" : "Expense settlement failed" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error settling expense");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Delete expense
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense([FromRoute] int id, [FromQuery] int userId)
        {
            try
            {
                var result = await _expenseService.DeleteExpenseAsync(id, userId);
                return Ok(new { success = result, message = result ? "Expense deleted successfully" : "Expense deletion failed" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expense");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get expense summary for trip
        /// </summary>
        [HttpGet("trip/{tripId}/summary")]
        public async Task<IActionResult> GetExpenseSummary([FromRoute] int tripId)
        {
            try
            {
                var summary = await _expenseService.GetExpenseSummaryAsync(tripId);
                return Ok(summary);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching expense summary");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}
