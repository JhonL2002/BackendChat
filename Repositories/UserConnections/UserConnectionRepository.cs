using BackendChat.Data;
using BackendChat.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendChat.Repositories.UserConnections
{
    public class UserConnectionRepository : IUserConnectionRepository
    {
        private readonly AppDbContext _dbContext;
        public UserConnectionRepository(AppDbContext appDbContext)
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
