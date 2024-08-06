using BackendChat.Data;
using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Services.BlobStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BackendChat.Services
{
    public class AccountService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountService> _logger;
        private readonly BlobImageService _blobService;
        public AccountService(AppDbContext appDbContext,
            IConfiguration configuration,
            ILogger<AccountService> logger,
            BlobImageService blobService
        )
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
            _logger = logger;
            _blobService = blobService;
        }

        public async Task RegisterAsync(RegisterDTO model)
        {
            var findUser = await GetUser(model.Email);
            if (findUser != null)
            {
                _logger.LogWarning("User already exists");
            }

            //Put a default URL image if not exists
            var defaultImageUrl = _blobService.GetDefaultImageUrl();

            _appDbContext.Users.Add(
                new AppUser()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Nickname = model.Nickname,
                    Email = model.Email,
                    DOB = model.DOB,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    ProfilePictureUrl = model.ProfilePictureUrl
                });

            await _appDbContext.SaveChangesAsync();
            _logger.LogInformation("**** Success: User created successfully! ****");
        }

        public async Task<string?> LoginAsync(LoginDTO model)
        {
            var finduser = await GetUser(model.Email);
            if (finduser == null)
            {
                _logger.LogError("**** User not found! ****");
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, finduser.Password))
            {
                _logger.LogError("**** Email/Password invalid! ****");
                return null;
            }

            string jwtToken = GenerateToken(finduser);
            _logger.LogInformation($"**** Login Succesfully **** {jwtToken}");
            return jwtToken;
        }

        private string GenerateToken(AppUser user)
        {
            user.FullName = $"{user.LastName}"+" "+$"{user.FirstName}";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Chat:JwtKey"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Nickname)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Chat:JwtIssuer"],
                audience: _configuration["Chat:JwtAudience"],
                claims: userClaims,
                expires: DateTime.Now.AddDays(2),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void RefreshToken(UserSession userSession)
        {
            CustomUserClaims customUserClaims = DecryptJwtService.DecryptToken(userSession.JwtToken);
            if (customUserClaims is null)
            {
                _logger.LogError("**** Invalid Token! ****");
            }
            string newToken = GenerateToken(new AppUser()
            { FirstName = customUserClaims.Name, Email = customUserClaims.Email, Nickname = customUserClaims.Identifier });
            _logger.LogInformation($"**** New token: {newToken} ****");
        }

        private async Task<AppUser> GetUser(string email) => await _appDbContext.Users.FirstOrDefaultAsync(e => e.Email == email);
    }
}
