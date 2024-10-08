using BackendChat.Data;
using BackendChat.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendChat.Repositories.UserConnections
{
    public class UserConnectionContext : IUserConnectionContext
    {
        private readonly AppDbContext _dbContext;
        public UserConnectionContext(AppDbContext appDbContext)
        {
            _dbContext = appDbContext;
        }
        public async Task<IEnumerable<string>> GetUserConnectionsAsync(string userId)
        {
            return await _dbContext.UserConnections
                .Where(uc => uc.UserId == Convert.ToInt32(userId))
                .Select(uc => uc.ConnectionId)
                .ToListAsync();
        }
    }
}
