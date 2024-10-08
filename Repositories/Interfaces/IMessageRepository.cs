using BackendChat.DTOs;
using BackendChat.Responses;

namespace BackendChat.Repositories.Interfaces
{
    public interface IMessageRepository
    {
        Task RegisterMessageAsync(ChatMessageDTO model);
        Task<List<ChatMediaResponse>> GetMessagesAsync(int chatId, int? lastMessageId = null, int pageSize = 6);
    }
}
