using BackendChat.Data;
using BackendChat.DTOs;
using BackendChat.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendChat.Services
{
    public class AccountService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountService> _logger;
        public AccountService(AppDbContext appDbContext ,IConfiguration configuration, ILogger<AccountService> logger)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task RegisterAsync(RegisterDTO model)
        {
            var findUser = await GetUser(model.Email);
            if (findUser != null)
            {
                _logger.LogWarning("User already exists");
            }

            _appDbContext.Users.Add(
                new AppUser()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    DOB = model.DOB,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
                });

            await _appDbContext.SaveChangesAsync();
            _logger.LogInformation("Success");
        }

        private async Task<AppUser> GetUser(string email) => await _appDbContext.Users.FirstOrDefaultAsync(e => e.Email == email);
    }
}
