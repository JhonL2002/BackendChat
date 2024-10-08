using BackendChat.DTOs.Chats;
using BackendChat.Responses;

namespace BackendChat.Repositories.Interfaces
{
    public interface IGroupRepository
    {
        Task<List<ChatDto>> GetAllGroupsFromCurrentUserAsync();
        Task<List<GroupResponse>> GetAllGroupsToJoinAsync();
        Task AddCurrentUserToGroupAsync(int chatId);
        Task<int> CreateGroupAsync(GroupDTO model);
    }
}
