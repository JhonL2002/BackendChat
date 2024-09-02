namespace BackendChat.Models
{
    public class UserConnection
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string ConnectionId { get; set; }

        public DateTime ConnectedAt { get; set; }

        public AppUser User { get; set; }
    }
}
