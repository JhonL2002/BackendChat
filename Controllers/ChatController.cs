using BackendChat.DTOs;
using BackendChat.DTOs.Chats;
using BackendChat.Hubs;
using BackendChat.Models;
using BackendChat.Responses;
using BackendChat.Services.BlobStorage;
using BackendChat.Services.ChatServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BackendChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatMessageService _chatMessageService;
        private readonly ManageGroupService _manageGroupService;
        private readonly UserContextService _userContextService;

        public ChatController(
            ChatMessageService chatMessageService,
            ManageGroupService manageGroupService,
            UserContextService userContextService
        )
        {
            _chatMessageService = chatMessageService;
            _manageGroupService = manageGroupService;
            _userContextService = userContextService;
        }

        //Send a chat with permmitted mediafiles
        [HttpPost("send-message")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromForm] ChatMessageDTO message)
        {
            await _chatMessageService.RegisterMessageAsync(message);
            var response = new MessageResponse
            {
                MediaUrl = message.MediaUrl
            };
            return Ok(response);
        }

        //Get all groups
        [HttpGet("get-groups")]
        [Authorize]
        public async Task<ActionResult<List<Chat>>> GetAllGroups()
        {
            var groups = await _userContextService.GetAllGroupsAsync();
            return Ok(groups);
        }

        //Create a group in chat
        [HttpPost("create-group")]
        [Authorize]
        public async Task<IActionResult> CreateGroup([FromBody] GroupDTO model)
        {
            var create = await _manageGroupService.CreateGroupAsync(model);
            return Ok(create);
        }

        //Join a user in a group
        [HttpPost("join-group")]
        [Authorize]
        public async Task<IActionResult> JoinUserToGroup(int chatId)
        {
            await _userContextService.AddChatParticipantToGroupAsync(chatId);
            return Ok();
        }

        //Get the user chats
        [HttpGet("user-chats")]
        [Authorize]
        public async Task<ActionResult<List<ChatDto>>> GetUserChats()
        {
            var getChats = await _userContextService.GetUserChatsAsync();
            return Ok(getChats);
        }

        //Get the messages from a chat
        [HttpGet("{chatId}/messages")]
        [Authorize]
        public async Task<ActionResult<List<ChatMediaResponse>>> GetMessages(int chatId, [FromQuery] int? lastMessageId)
        {
            try
            {
                var getChats = await _chatMessageService.GetMessagesWithUpdatedSaSUrlsAsync(chatId, lastMessageId);
                return Ok(getChats);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message, "Failed to get messages with updated SAS Urls");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
