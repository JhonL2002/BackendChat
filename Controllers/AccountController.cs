using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Services;
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

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> OnRegister(RegisterDTO model)
        {
            await _accountService.RegisterAsync(model);
            return Ok(model);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> OnLogin(LoginDTO model)
        {
            await _accountService.LoginAsync(model);
            return Ok(model);
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
