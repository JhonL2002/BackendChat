using BackendChat.DTOs;
using BackendChat.Repositories.Interfaces;
using BackendChat.Responses;
using BackendChat.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUploadImageService _blobService;
        private readonly ILogger<AccountController> _logger;
        private readonly ILoginRepository _loginRepository;

        public AccountController(
            IUserRepository userRepository,
            IUploadImageService blobImageService,
            ILogger<AccountController> logger,
            ILoginRepository loginRepository)
        {
            _userRepository = userRepository;
            _blobService = blobImageService;
            _logger = logger;
            _loginRepository = loginRepository;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> OnRegister([FromForm] RegisterDTO model)
        {
            try
            {
                await _userRepository.RegisterAsync(model);
                var response = new RegisterResponse
                {
                    ProfilePictureUrl = model.ProfilePictureUrl,
                    Token = model.EmailConfirmationToken
                };
                return Ok(response);
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
            
        }

        [HttpPost("confirm-email")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmEmail(string userNickname, string token, ConfirmEmailDto model)
        {
            try
            {
                var user = await _userRepository.GetUserByNicknameAsync(userNickname);
                if (user == null || user.EmailConfirmationToken != token)
                {
                    return BadRequest();
                }

                await _userRepository.SetConfirmationEmailAsync(user.Id, user);
                return Ok();
            }
            catch(Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
            
        }

        /*[Authorize]
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
        }*/

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> OnLogin(LoginDTO model)
        {
            try
            {
                var token = await _loginRepository.LoginAsync(model);
                if (token == null)
                {
                    return Unauthorized();
                }
                return Ok(new { Token = token });
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }

        }

        /*[HttpPost("refresh-token")]
        [AllowAnonymous]
        public IActionResult RefreshToken (UserSession model)
        {
            _loginRepository.RefreshToken(model);
            return Ok(model);
        }*/
    }
}
