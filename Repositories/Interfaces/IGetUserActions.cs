using BackendChat.Models;

namespace BackendChat.Repositories.Interfaces
{
    public interface IGetUserActions
    {
        int GetUserId();
        Task<AppUser> GetUserByEmailAsync(string email);
        Task<AppUser?> GetUserByNicknameAsync(string nickName);
    }
}
