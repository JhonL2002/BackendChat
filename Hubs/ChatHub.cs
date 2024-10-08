using BackendChat.Data;
using BackendChat.Models;
using BackendChat.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BackendChat.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IGroupRepository _groupRepository;
        private readonly AppDbContext _appDbContext;
        private readonly IUserConnectionContext _userConnectionContext;

        public ChatHub(IGroupRepository groupRepository,
            AppDbContext appDbContext,
            IUserConnectionContext userConnectionContext)
        {
            _groupRepository = groupRepository;
            _appDbContext = appDbContext;
            _userConnectionContext = userConnectionContext;
        }

        //Method to create a group
        public async Task CreateGroup(string groupName)
        {
            //Get the user identifier
            var userId = Context.UserIdentifier;
            await Clients.All.SendAsync("Group Created", userId, groupName);
        }

        //Method to join to group
        [Authorize]
        public async Task JoinGroup(int chatId)
        {
            var userId = Context.UserIdentifier;

            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User identifier is null");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("User identifier is null!");
                return;
            }

            //Get all user connections from service
            var connectionIds = await _userConnectionContext.GetUserConnectionsAsync(userId);
            if (connectionIds == null || !connectionIds.Any())
            {
                await Clients.Caller.SendAsync("Error", "No active connections found for the user");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No active connections found for the user!");
                return;
            }
            foreach (var connectionId in connectionIds)
            {
                try
                {
                    await Groups.AddToGroupAsync(connectionId, chatId.ToString());
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"Connection {connectionId} added to group {chatId} successfully!");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to add connection {connectionId} to group {chatId}. Error: {ex.Message}");
                }
            }

            await Clients.Caller.SendAsync("JoinedGroup", chatId.ToString());
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"User {userId} joined group {chatId} successfully!");
        }

        //Send Messages to group
        public async Task SendMessageToGroup(int chatId, string userName, string? message, string? mediaUrl)
        {
            await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", userName, message, mediaUrl);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"User {userName} send {message} successfully!");
        }

        //Set the connection to database
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            var userConnection = new UserConnection
            {
                UserId = Convert.ToInt32(userId),
                ConnectionId = connectionId,
                ConnectedAt = DateTime.UtcNow
            };

            _appDbContext.UserConnections.Add(userConnection);
            await _appDbContext.SaveChangesAsync();
        }

        //Delete the connection from database
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            var userConnection = await _appDbContext.UserConnections
                .FirstOrDefaultAsync(uc => uc.ConnectionId == connectionId && uc.UserId == Convert.ToInt32(userId));
            if (userConnection != null)
            {
                try
                {
                    _appDbContext.UserConnections.Remove(userConnection);
                    await _appDbContext.SaveChangesAsync();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Connection {connectionId} removed for user {userId}");

                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error removing connection {connectionId}: {ex.Message}");
                }
                
            }
        }
    }
}
