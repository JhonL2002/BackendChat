
namespace BackendChat.Models
{
    public class ChatParticipant
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; }


        //An user can be stay in one chat, but can participate in many chats
        public Chat Chat { get; set; }
        public AppUser User { get; set; }
    }
}
