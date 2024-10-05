using BackendChat.DTOs;

namespace BackendChat.Repositories.Interfaces
{
    public interface ILoginRepository
    {
        Task<string?> LoginAsync(LoginDTO model);
    }
}
