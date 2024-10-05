/*using BackendChat.Data;
using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Services.BlobStorage;
using Microsoft.EntityFrameworkCore;
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

        public async Task UpdateAsync(int id, UpdateUserDto model)
        {
            var user = await _appDbContext.Users.FindAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User not found");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Nickname = model.Nickname;
            user.DOB = model.DOB;

            if (model.ProfilePicture != null)
            {
                var profilePictureUrl = await _blobService.UploadProfileImageAsync(model.ProfilePicture);

                //Put delete picture profile method here if is necessary!!

                user.ProfilePictureUrl = profilePictureUrl;
            }

            _appDbContext.Users.Update(user);
            await _appDbContext.SaveChangesAsync();
            _logger.LogInformation("User updated successfully!");
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

            if (!finduser.EmailConfirmed)
            {
                _logger.LogError("**** Email not confirmed! ****");
                return null;
            }

            string jwtToken = GenerateToken(finduser);
            _logger.LogInformation($"**** Login Succesfully **** {jwtToken}");
            return jwtToken;
        }

        private string GenerateToken(AppUser user)
        {
            user.FullName = $"{user.FirstName}"+" "+$"{user.LastName}";
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
*/