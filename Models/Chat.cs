namespace BackendChat.Models
{
    public class Chat
    {
        public int ChatId { get; set; }

        public string ChatName { get; set; }

        public int ChatTypeId { get; set; }

        //Optional property to determine who creted the group
        public int? CreatedByUserId { get; set; }

        //A chat only can have one chat type
        public ChatType ChatType { get; set; }

        //The user who created the group
        public AppUser CreatedBy { get; set; }

        //A chat can have many participants, but only one participant can belong to a chat at time
        public ICollection<ChatParticipant> ChatParticipants { get; set; }
        
        //A chat can have many messages, but each message belongs to unique chat
        public ICollection<ChatMessage> ChatMessages { get; set; }
    }
}
