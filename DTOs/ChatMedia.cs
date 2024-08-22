namespace BackendChat.DTOs
{
    public class ChatMedia
    {
        public string User {  get; set; }
        public string? MediaUrl { get; set; }
        public IFormFile File { get; set; }
    }
}
