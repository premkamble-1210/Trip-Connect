namespace APPLICATION_LAYER.DTOs.JoinRequest
{
    /// <summary>
    /// Update Join Request DTO - Accept or Reject
    /// </summary>
    public class UpdateJoinRequestDto
    {
        public int RequestId { get; set; }
        public string Status { get; set; } // "Accepted" or "Rejected"
    }
}
