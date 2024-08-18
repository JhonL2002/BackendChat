using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Responses;
using BackendChat.Services;
using BackendChat.Services.BlobStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BackendChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly BlobImageService _blobService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AccountService accountService, BlobImageService blobImageService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _blobService = blobImageService;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> OnRegister([FromForm] RegisterDTO model)
        {
            if (model.ProfilePicture != null)
            {
                //Upload an image and get the url
                var imageUrl = await _blobService.UploadProfileImageAsync(model.ProfilePicture);
                model.ProfilePictureUrl = imageUrl;
            }
            else
            {
                //Assign a default image
                model.ProfilePictureUrl = _blobService.GetDefaultImageUrl();
            }
            await _accountService.RegisterAsync(model);
            var response = new RegisterResponse
            {
                ProfilePictureUrl = model.ProfilePictureUrl,
                Token = model.EmailConfirmationToken
            };
            return Ok(response);
        }

        [HttpPost("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userNickname, string token, ConfirmEmailDto model)
        {
            var user = await _accountService.GetUserByNicknameAsync(userNickname);
            if (user == null || user.EmailConfirmationToken != token)
            {
                return BadRequest("Invalid confirmation token");
            }
            user.EmailConfirmed = model.EmailConfirmed;
            user.EmailConfirmationToken = null;

            await _accountService.UpdateAfterRegisterAsync(user.Id, user);

            _logger.LogInformation("Email successfully confirmed");
            return Ok(new { message = "Email successfully confirmed", userId = user.Id });
        }

        [Authorize]
        [HttpPut("update/{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> OnUpdate(int id, [FromForm] UpdateUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.ProfilePicture != null)
            {
                var profilePictureUrl = await _blobService.UploadProfileImageAsync(model.ProfilePicture);
                model.ProfilePictureUrl = profilePictureUrl;
            }

            await _accountService.UpdateAsync(id, model);
            return NoContent();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> OnLogin(LoginDTO model)
        {
            var token = await _accountService.LoginAsync(model);
            if (token == null)
            {
                return Unauthorized();
            }
            return Ok(new { Token = token});
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public IActionResult RefreshToken (UserSession model)
        {
            _accountService.RefreshToken(model);
            return Ok(model);
        }
    }
}
