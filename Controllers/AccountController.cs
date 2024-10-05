﻿using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Repositories.Interfaces;
using BackendChat.Responses;
using BackendChat.Responses.BackendChat.Responses;
using BackendChat.Services;
using BackendChat.Services.BlobStorage;
using BackendChat.Services.Interfaces;
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
        private readonly IUserRepository _userRepository;
        private readonly IBlobImageService _blobService;
        private readonly ILogger<AccountController> _logger;
        private readonly ILoginRepository _loginRepository;

        public AccountController(
            IUserRepository userRepository,
            IBlobImageService blobImageService,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering the user.");
                return StatusCode(500, "Internal server error.");
            }
            
        }

        [HttpPost("confirm-email")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmEmail(string userNickname, string token, ConfirmEmailDto model)
        {
            var user = await _userRepository.GetUserByNicknameAsync(userNickname);
            if (user == null || user.EmailConfirmationToken != token)
            {
                var failureResponse = new FailureResponse("Invalid confirmation token");
                return BadRequest(failureResponse);
            }

            await _userRepository.SetConfirmationEmailAsync(user.Id, user);
            var response = new SuccessResponse("Email confirmed successfully!");
            _logger.LogInformation(response.ToString());
            return Ok(response);
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
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FailureResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> OnLogin(LoginDTO model)
        {
            var token = await _loginRepository.LoginAsync(model);
            if (token == null)
            {
                var failureResponse = new FailureResponse("Unauthorized access, please verify credentials");
                return Unauthorized(failureResponse);
            }
            var successResponse = new SuccessResponse("Loged in successfully!");
            return Ok(successResponse);
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
