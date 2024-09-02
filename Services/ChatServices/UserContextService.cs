using BackendChat.Data;
using BackendChat.DTOs.Chats;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace BackendChat.Services.ChatServices
{
    public class UserContextService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IHttpContextAccessor _contextAccessor;

        public UserContextService(AppDbContext appDbContext, IHttpContextAccessor contextAccessor)
        {
            _appDbContext = appDbContext;
            _contextAccessor = contextAccessor;
        }

        public async Task<IEnumerable<string>> GetUserConnectionsAsync(string userId)
        {
            return await _appDbContext.UserConnections
                .Where(uc => uc.UserId == Convert.ToInt32(userId))
                .Select(uc => uc.ConnectionId)
                .ToListAsync();
        }

        public async Task<List<ChatDto>> GetUserChatsAsync()
        {
            var httpContext = _contextAccessor.HttpContext;
            var userIdClaim = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID is not available");
            }

            var chats = await (from cp in _appDbContext.ChatParticipants
                               join c in _appDbContext.Chats on cp.ChatId equals c.ChatId
                               join ct in _appDbContext.ChatTypes on c.ChatTypeId equals ct.ChatTypeId
                               where cp.UserId == userId
                               select new ChatDto
                               {
                                   ChatId = cp.ChatId,
                                   ChatName = c.ChatName,
                                   ChatTypeId = c.ChatTypeId
                               }).ToListAsync();
            return chats;
        }


    }
}
