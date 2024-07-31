using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Services;
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
        public async Task<IActionResult> OnRegister(RegisterDTO model)
        {
            await _accountService.RegisterAsync(model);
            return Ok(model);
        }
    }
}
