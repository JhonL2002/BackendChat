using BackendChat.Data;
using BackendChat.DTOs.Chats;
using BackendChat.Models;
using BackendChat.Repositories.Interfaces;
using BackendChat.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace BackendChat.Repositories.ChatRepository
{
    public class GroupRepository : IGroupRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IHttpContextAccessor _contextAccessor;

        public GroupRepository(AppDbContext appDbContext, IHttpContextAccessor contextAccessor)
        {
            _appDbContext = appDbContext;
            _contextAccessor = contextAccessor;
        }

        //Get all groups from current user
        public async Task<List<ChatDto>> GetAllGroupsFromCurrentUserAsync()
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
                               where cp.UserId == userId && c.ChatTypeId == 3
                               select new ChatDto
                               {
                                   ChatId = cp.ChatId,
                                   ChatName = c.ChatName,
                                   ChatTypeId = c.ChatTypeId
                               }).ToListAsync();
            return chats;
        }

        //Get all available groups to join
        public async Task<List<GroupResponse>> GetAllGroupsToJoinAsync()
        {
            var httpContext = _contextAccessor.HttpContext;
            var userIdClaim = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID is not available");
            }

            var chatGroups = await (from c in _appDbContext.Chats
                                    join cp in _appDbContext.ChatParticipants
                                    on c.ChatId equals cp.ChatId
                                    where !(from cpInner in _appDbContext.ChatParticipants
                                            where cpInner.UserId == userId && c.ChatTypeId == 3
                                            select cpInner.ChatId).Contains(c.ChatId)
                                    select new GroupResponse
                                    {
                                        ChatId = c.ChatId,
                                        ChatName = c.ChatName
                                    }).ToListAsync();

            return chatGroups;
        }

        //Add the current user to a chat
        public async Task AddCurrentUserToGroupAsync(int chatId)
        {
            var httpContext = _contextAccessor.HttpContext;

            var userIdClaim = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID is not available");
            }

            var chatParticipant = new ChatParticipant
            {
                ChatId = chatId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            _appDbContext.ChatParticipants.Add(chatParticipant);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<int> CreateGroupAsync(GroupDTO model)
        {
            var httpContext = _contextAccessor.HttpContext;

            var userIdClaim = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID is not available");
            }
            //Create the group
            var group = new Chat
            {
                ChatName = model.GroupName,
                ChatTypeId = (int)ChatTypeEnum.Group,
                CreatedByUserId = userId,
            };

            _appDbContext.Chats.Add(group);
            await _appDbContext.SaveChangesAsync();

            //Add the creator to database and group
            var participant = new ChatParticipant
            {
                ChatId = group.ChatId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            _appDbContext.ChatParticipants.Add(participant);
            await _appDbContext.SaveChangesAsync();
            return group.ChatId;

        }

    }
}
