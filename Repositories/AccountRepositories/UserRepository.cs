using BackendChat.Data;
using BackendChat.DTOs;
using BackendChat.Helpers;
using BackendChat.Models;
using BackendChat.Repositories.Interfaces;
using BackendChat.Responses;
using BackendChat.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackendChat.Repositories.UserAccount
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<IUserRepository> _logger;
        private readonly ISendEmailService _sendEmail;
        private readonly IUploadImageService _blobImageService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUploadImageService _uploadImageService;

        public UserRepository(
            AppDbContext dbContext,
            ILogger<IUserRepository> logger,
            ISendEmailService sendEmail,
            IUploadImageService blobImageService,
            IHttpContextAccessor contextAccessor,
            IUploadImageService uploadImageService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _sendEmail = sendEmail;
            _blobImageService = blobImageService;
            _contextAccessor = contextAccessor;
            _uploadImageService = uploadImageService;
        }

        public async Task<AppUser> GetUser(string email) =>
            await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(e => e.Email == email);

        public async Task<AppUser?> GetUserByNicknameAsync(string nickName)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Nickname == nickName);
        }

        public async Task<UpdateUserDTO> GetUserDataAsync()
        {
            var httpContext = _contextAccessor.HttpContext;
            var userIdClaim = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID is not available");
            }

            var user = await _dbContext.Users.FindAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found");
            }

            var userDTO = new UpdateUserDTO
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Nickname = user.Nickname,
                Email = user.Email,
                DOB = user.DOB,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return userDTO;
        }

        public async Task RegisterAsync(RegisterDTO model)
        {
            var findUser = await GetUser(model.Email);
            var findNickname = await GetUserByNicknameAsync(model.Nickname);
            if (findUser != null && findNickname != null)
            {
                throw new InvalidOperationException("User already exists");
            }

            //Upload an image and assign an URL
            if (model.ProfilePicture != null)
            {
                model.ProfilePictureUrl = await _blobImageService.UploadProfileImageAsync(model.ProfilePicture);
            }
            else
            {
                model.ProfilePictureUrl = _blobImageService.GetDefaultImageUrl();
            }

            model.EmailConfirmationToken = GenerateGuidCode.GenerateGuidToken();

            _dbContext.Users.Add(
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
            await _dbContext.SaveChangesAsync();
            await _sendEmail.SendConfirmationEmailAsync(model);
            _logger.LogInformation("**** Success: User created successfully! ****");
        }

        public async Task SetConfirmationEmailAsync(int id, AppUser model)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found");
            }
            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(UpdateUserDTO model)
        {
            var httpContext = _contextAccessor.HttpContext;
            var userIdClaim = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID is not available");
            }

            var user = await _dbContext.Users.FindAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found");
            }


            //Verify if the current email is different
            if (user.Email != model.Email)
            {
                var findUser = await GetUser(model.Email);
                if (findUser != null && findUser.Id != userId)
                {
                    throw new InvalidOperationException("Email already in use by another user.");
                }
                //User needs to re-confirm email
                user.Email = model.Email;
                user.EmailConfirmed = false;

                //Generate a GUID Token to send, update this in model
                model.EmailConfirmationToken = GenerateGuidCode.GenerateGuidToken();
                user.EmailConfirmationToken = model.EmailConfirmationToken;
                await _sendEmail.SendConfirmationEmailAsync(model);

                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Email sent succesfully!");
            }

            if (user.Nickname != model.Nickname)
            {
                var findNickname = await GetUserByNicknameAsync(model.Nickname);
                if (findNickname != null)
                {
                    throw new InvalidOperationException("Nickname already in use by another user");
                }
                user.Nickname = model.Nickname;
            }

            if (model.ProfilePicture != null)
            {
                var profilePictureUrl = await _blobImageService.UploadProfileImageAsync(model.ProfilePicture);

                //Put delete picture profile method here if is necessary!!

                user.ProfilePictureUrl = profilePictureUrl;
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.DOB = model.DOB;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User updated successfully!");
        }
    }
}
