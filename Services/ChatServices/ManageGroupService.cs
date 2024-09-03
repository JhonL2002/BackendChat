using BackendChat.Data;
using BackendChat.DTOs.Chats;
using BackendChat.Models;
using BackendChat.Responses;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace BackendChat.Services.ChatServices
{
    public class ManageGroupService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ManageGroupService(AppDbContext appDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _appDbContext = appDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> CreateGroupAsync(GroupDTO model)
        {
            //Create the group
            var group = new Chat
            {
                ChatName = model.GroupName,
                ChatTypeId = (int)ChatTypeEnum.Group,
                CreatedByUserId = model.CreatorUserId
            };

            if (group.CreatedByUserId == null)
            {
                throw new ArgumentNullException(nameof(group.CreatedByUserId), "CreateByUserId cannot be null");
            }

            _appDbContext.Chats.Add(group);
            await _appDbContext.SaveChangesAsync();

            //Add the creator to database and group
            var participant = new ChatParticipant
            {
                ChatId = group.ChatId,
                UserId = model.CreatorUserId,
                JoinedAt = DateTime.UtcNow
            };

            _appDbContext.ChatParticipants.Add(participant);
            await _appDbContext.SaveChangesAsync();
            return group.ChatId;

        }

        public async Task AddUserToGroupAsync(int groupId, string userId)
        {
            var participant = new ChatParticipant
            {
                ChatId = groupId,
                UserId = (int)Convert.ToInt64(userId),
                JoinedAt = DateTime.UtcNow
            };

            _appDbContext.ChatParticipants.Add(participant);
            await _appDbContext.SaveChangesAsync();
        }

        //Get all groups
        public async Task<List<GroupResponse>> GetAllGroupsAsync()
        {
            var chats = await _appDbContext.Chats
                .Where(c => c.ChatTypeId == (int)ChatTypeEnum.Group)
                .Select( c=> new GroupResponse
                {
                    ChatId = c.ChatId,
                    ChatName = c.ChatName
                })
                .ToListAsync();
            return chats;
        }

        //Get a group by id
        public async Task<Chat> GetGroupByIdAsync(int groupId)
        {
            var chatGroups = _appDbContext.Chats
                .Where(c => c.ChatId == groupId && c.ChatTypeId == (int)ChatTypeEnum.Group)
                .FirstOrDefaultAsync();
            return await chatGroups;
        }
    }
}
