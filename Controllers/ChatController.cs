using BackendChat.DTOs;
using BackendChat.Hubs;
using BackendChat.Responses;
using BackendChat.Services;
using BackendChat.Services.BlobStorage;
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
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly BlobMediaService _blobMediaService;

        public ChatController(IHubContext<ChatHub> hubContext, BlobMediaService blobMediaService)
        {
            _hubContext = hubContext;
            _blobMediaService = blobMediaService;
        }

        [HttpPost("send")]
        [AllowAnonymous]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.User, message.Text);
            return Ok(new { Status = "Message sent" });
        }

        [HttpPost("sendmedia")]
        [Consumes("multipart/form-data")]
        [AllowAnonymous]
        public async Task<IActionResult> SendMedia([FromForm] ChatMedia message)
        {
            if (message.File != null)
            {
                //Upload an image and get the url
                var imageUrl = await _blobMediaService.UploadMediaAsync(message.File);
                message.MediaUrl = imageUrl;
            }
            else
            {
                return BadRequest("Please put a valid file!");
            }
            await _hubContext.Clients.All.SendAsync("ReceiveMedia", message.User, message.MediaUrl);
            var response = new ChatMediaResponse
            {
                MediaUrl = message.MediaUrl,
                User = message.User
                
            };
            return Ok(response);
        }
    }
}
