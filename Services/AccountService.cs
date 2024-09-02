using BackendChat.Data;
using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Services.BlobStorage;
using BackendChat.Services.EmailSender;
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
        private readonly EmailService _emailservice;
        public AccountService(AppDbContext appDbContext,
            IConfiguration configuration,
            ILogger<AccountService> logger,
            BlobImageService blobService,
            EmailService emailService
        )
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
            _logger = logger;
            _blobService = blobService;
            _emailservice = emailService;
        }

        public async Task RegisterAsync(RegisterDTO model)
        {
            var findUser = await GetUser(model.Email);
            if (findUser != null)
            {
                _logger.LogWarning("User already exists");
            }

            //Put a default URL image if not exists
            _blobService.GetDefaultImageUrl();
            model.EmailConfirmationToken = GenerateEmailConfirmationToken();

            _appDbContext.Users.Add(
                new AppUser()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Nickname = model.Nickname,
                    Email = model.Email,
                    DOB = model.DOB,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    ProfilePictureUrl = model.ProfilePictureUrl,
                    EmailConfirmationToken = model.EmailConfirmationToken,
                });
            await _appDbContext.SaveChangesAsync();
            await SendConfirmationEmailAsync(model);
            _logger.LogInformation("**** Success: User created successfully! ****");
        }

        public async Task UpdateAfterRegisterAsync(int id, AppUser model)
        {
            var user = await _appDbContext.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found");
            }

            user.EmailConfirmed = model.EmailConfirmed;
            user.EmailConfirmationToken = model.EmailConfirmationToken;

            _appDbContext.Users.Update(user);
            await _appDbContext.SaveChangesAsync();
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

        /*public async Task<AppUser?> GetUserByIdAsync(int id)
        {
            return await _appDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }*/

        public async Task<AppUser?> GetUserByNicknameAsync(string nickName)
        {
            return await _appDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Nickname == nickName);
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
            user.FullName = $"{user.LastName}"+" "+$"{user.FirstName}";
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

        public async Task SendConfirmationEmailAsync(RegisterDTO user)
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"];
            var encodedToken = Uri.EscapeDataString(user.EmailConfirmationToken);
            var confirmationLink = $"{baseUrl}/confirm-email?userNickname={user.Nickname}&token={encodedToken}";
            string message = $"Please, confirm your account here {confirmationLink}";
            await _emailservice.SendEmailAsync(_configuration["EmailCredentials:FromEmail"]!, user.Email, "Confirm your email", message);

        }

        private async Task<AppUser> GetUser(string email) => await _appDbContext.Users.FirstOrDefaultAsync(e => e.Email == email);
        public string GenerateEmailConfirmationToken() => Guid.NewGuid().ToString();
    }
}
