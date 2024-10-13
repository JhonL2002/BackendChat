using BackendChat.Data;
using BackendChat.Models;
using BackendChat.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackendChat.Repositories.AccountRepositories
{
    public class GetUserActions : IGetUserActions
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly AppDbContext _dbContext;

        public GetUserActions(IHttpContextAccessor contextAccessor, AppDbContext dbContext)
        {
            _contextAccessor = contextAccessor;
            _dbContext = dbContext;
        }

        public int GetUserId()
        {
            var httpContext = _contextAccessor.HttpContext;
            var userIdClaim = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID is not available");
            }
            return userId;
        }

        public async Task<AppUser> GetUserByEmailAsync(string email) =>
            await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(e => e.Email == email);

        public async Task<AppUser?> GetUserByNicknameAsync(string nickName)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Nickname == nickName);
        }
    }
}
