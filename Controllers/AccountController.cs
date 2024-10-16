using BackendChat.DTOs;
using BackendChat.Repositories.AccountRepositories;
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
        private readonly IGetBlobActionsService _blobService;
        private readonly ILogger<AccountController> _logger;
        private readonly ILoginRepository _loginRepository;
        private readonly IGetUserActions _getUserActions;

        public AccountController(
            IUserRepository userRepository,
            IGetBlobActionsService blobImageService,
            ILogger<AccountController> logger,
            ILoginRepository loginRepository,
            IGetUserActions getUserActions)
        {
            _userRepository = userRepository;
            _blobService = blobImageService;
            _logger = logger;
            _loginRepository = loginRepository;
            _getUserActions = getUserActions;
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
                var user = await _getUserActions.GetUserByNicknameAsync(userNickname);
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

        [Authorize]
        [HttpGet("get-user")]
        public async Task<IActionResult> GetUser()
        {
            var userDTO = await _userRepository.GetUserDataAsync();
            return Ok(userDTO);
        }

        [Authorize]
        [HttpPut("update-user")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> OnUpdate([FromForm] UpdateUserDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userRepository.UpdateAsync(model);
            return NoContent();
        }

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
