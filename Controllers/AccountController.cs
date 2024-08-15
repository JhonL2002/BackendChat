using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Responses;
using BackendChat.Services;
using BackendChat.Services.BlobStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly BlobImageService _blobService;

        public AccountController(AccountService accountService, BlobImageService blobImageService)
        {
            _accountService = accountService;
            _blobService = blobImageService;
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
            };
            return Ok(response);
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
