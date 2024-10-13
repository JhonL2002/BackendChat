using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Repositories.Interfaces;

namespace BackendChat.Strategies.Interfaces
{
    public interface IUserUpdateStrategy
    {
        Task UpdateUserAsync(UpdateUserDTO model, AppUser user);
    }
}
