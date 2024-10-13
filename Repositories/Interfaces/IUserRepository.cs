using BackendChat.DTOs;
using BackendChat.Models;
using System.Threading.Tasks;

namespace BackendChat.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task RegisterAsync(RegisterDTO model);
        Task SetConfirmationEmailAsync(int id, AppUser model);
        Task UpdateAsync(UpdateUserDTO model);
        Task<UpdateUserDTO> GetUserDataAsync();
    }
}
