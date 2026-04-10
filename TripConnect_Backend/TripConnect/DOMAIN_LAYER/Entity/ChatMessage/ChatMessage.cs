namespace DOMAIN_LAYER.Entity.ChatMessage
{
    /// <summary>
    /// ChatMessage Entity - Represents a message in trip group chat
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Unique identifier for the chat message
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Trip ID this message belongs to
        /// </summary>
        public int TripId { get; set; }

        /// <summary>
        /// User ID of the message sender
        /// </summary>
        public int SenderId { get; set; }

        /// <summary>
        /// Message content
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Timestamp when the message was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Navigation property - Trip this message belongs to (1:M)
        /// </summary>
        public DOMAIN_LAYER.Entity.Trip.Trip Trip { get; set; }

        /// <summary>
        /// Navigation property - User who sent this message (1:M)
        /// </summary>
        public DOMAIN_LAYER.Entity.User.User Sender { get; set; }
    }
}
