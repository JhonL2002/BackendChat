using BackendChat.DTOs;
using BackendChat.Helpers.Interfaces;
using BackendChat.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BackendChat.Helpers
{
    public class AdminTokenCode : IAdminTokenCode
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IAdminTokenCode> _logger;

        public AdminTokenCode(IConfiguration configuration, ILogger<IAdminTokenCode> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateToken(AppUser user)
        {
            user.FullName = $"{user.FirstName}" + " " + $"{user.LastName}";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Chat:JwtKey"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
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
    }
}
