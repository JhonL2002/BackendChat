namespace BackendChat.DTOs
{
    public class ChatMessageDTO
    { 
        public int UserId {  get; set; }
        public string? UserName { get; set; }
        public int ChatId { get; set; }
        public string? Text { get; set; }
        public string? MediaUrl { get; set; }
        public IFormFile? File { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
