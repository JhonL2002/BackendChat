using BackendChat.DTOs;
using BackendChat.Models;

namespace BackendChat.Helpers.Interfaces
{
    public interface IAdminTokenCode
    {
        string GenerateToken(AppUser user);
        void RefreshToken(UserSession userSession);
    }
}
