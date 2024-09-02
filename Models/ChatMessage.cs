namespace BackendChat.Models
{
    public class ChatMessage
    {
        public int MessageId { get; set; }
        public int UserId {  get; set; }

        public int ChatId { get; set; }

        public string? Text { get; set; }

        public string? MediaUrl { get; set; }

        public DateTime? Timestamp { get; set; } = DateTime.UtcNow;

        //Each message belongs to unique chat
        //A chat can have many messages
        public Chat Chat { get; set; }

        //Each message was sent by an unique user
        //An user can send many messages
        public AppUser User { get; set; }
    }
}
