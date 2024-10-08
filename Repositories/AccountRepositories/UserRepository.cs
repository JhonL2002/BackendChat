using BackendChat.Data;
using BackendChat.DTOs;
using BackendChat.Helpers;
using BackendChat.Models;
using BackendChat.Repositories.Interfaces;
using BackendChat.Responses;
using BackendChat.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendChat.Repositories.UserAccount
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<IUserRepository> _logger;
        private readonly ISendEmailService _sendEmail;
        private readonly IUploadImageService _blobImageService;

        public UserRepository(
            AppDbContext dbContext,
            ILogger<IUserRepository> logger,
            ISendEmailService sendEmail,
            IUploadImageService blobImageService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _sendEmail = sendEmail;
            _blobImageService = blobImageService;
        }

        public async Task<AppUser> GetUser(string email) =>
            await _dbContext.Users.FirstOrDefaultAsync(e => e.Email == email);

        public async Task<AppUser?> GetUserByNicknameAsync(string nickName)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Nickname == nickName);
        }

        public async Task RegisterAsync(RegisterDTO model)
        {
            var findUser = await GetUser(model.Email);
            if (findUser != null)
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
    }
}
