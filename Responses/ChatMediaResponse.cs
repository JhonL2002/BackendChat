namespace BackendChat.Responses
{
    public class ChatMediaResponse
    {
        public int MessageId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int ChatId { get; set; }
        public string? Text { get; set; }
        public string? MediaUrl { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
