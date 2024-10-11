using BackendChat.DTOs;
using BackendChat.Models;
using System.Threading.Tasks;

namespace BackendChat.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task RegisterAsync(RegisterDTO model);
        Task<AppUser> GetUser(string email);
        Task SetConfirmationEmailAsync(int id, AppUser model);
        Task<AppUser?> GetUserByNicknameAsync(string nickName);
        Task UpdateAsync(UpdateUserDTO model);
        Task<UpdateUserDTO> GetUserDataAsync();
    }
}
