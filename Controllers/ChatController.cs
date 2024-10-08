using BackendChat.DTOs;
using BackendChat.DTOs.Chats;
using BackendChat.Models;
using BackendChat.Repositories.Interfaces;
using BackendChat.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupRepository _currentUserRepository;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IMessageRepository messageRepository,
            IGroupRepository groupRepository,
            IGroupRepository currentUserRepository,
            ILogger<ChatController> logger
        )
        {
            _currentUserRepository = currentUserRepository;
            _groupRepository = groupRepository;
            _messageRepository = messageRepository;
            _logger = logger;
        }

        //Send a chat with permmitted mediafiles
        [HttpPost("send-message")]
        [Consumes("multipart/form-data")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendMessage([FromForm] ChatMessageDTO message)
        {
            if (message == null)
            {
                return BadRequest("Please, verify data");
            }
            try
            {
                await _messageRepository.RegisterMessageAsync(message);
                var mediaUrl = new MessageResponse
                {
                    MediaUrl = message.MediaUrl
                };
                _logger.LogInformation("Message sent successfully!");
                return Ok(mediaUrl);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("You are not authorized to view this resource");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //Get all groups to join
        [HttpGet("get-groups")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Chat>>> GetAllGroups()
        {
            try
            {
                var groups = await _currentUserRepository.GetAllGroupsToJoinAsync();
                if (groups == null || groups.Count == 0)
                {
                    return NotFound("No groups found to join");
                }
                return Ok(groups);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("You are not authorized to view this resource");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            
        }

        //Create a group in chat
        [HttpPost("create-group")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateGroup([FromBody] GroupDTO model)
        {
            try
            {
                var create = await _groupRepository.CreateGroupAsync(model);
                return Ok(create);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        //Join in a group from current user
        [HttpPost("join-group")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> JoinUserToGroup(int chatId)
        {
            try
            {
                await _currentUserRepository.AddCurrentUserToGroupAsync(chatId);
                return Ok("Group created successfully!");
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        //Get all groups from current user
        [HttpGet("user-chats")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ChatDto>>> GetUserChats()
        {
            try
            {
                var groups = await _currentUserRepository.GetAllGroupsFromCurrentUserAsync();
                if (groups == null || groups.Count == 0)
                {
                    return NotFound("You don't belong to any group!");
                }
                return Ok(groups);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("You are not authorized to view this resource");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //Get the messages from a chat
        [HttpGet("{chatId}/messages")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ChatMediaResponse>>> GetMessages(int chatId, [FromQuery] int? lastMessageId)
        {
            try
            {
                var getChats = await _messageRepository.GetMessagesAsync(chatId, lastMessageId);
                return Ok(getChats);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}