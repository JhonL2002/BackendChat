namespace BackendChat.Models
{
    public class ChatType
    {
        public int ChatTypeId { get; set; }

        public string ChatTypeName { get; set; }

        public ICollection<Chat> Chats { get; set; }
    }
}
