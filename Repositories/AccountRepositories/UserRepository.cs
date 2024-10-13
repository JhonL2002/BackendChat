using BackendChat.Data;
using BackendChat.DTOs;
using BackendChat.Helpers;
using BackendChat.Models;
using BackendChat.Repositories.AccountRepositories;
using BackendChat.Repositories.Interfaces;
using BackendChat.Responses;
using BackendChat.Services.Interfaces;
using BackendChat.Strategies.Implementations;
using BackendChat.Strategies.Interfaces;
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
        private readonly IGetUserActions _getUserActions;

        public UserRepository(
            AppDbContext dbContext,
            ILogger<IUserRepository> logger,
            ISendEmailService sendEmail,
            IUploadImageService blobImageService,
            IGetUserActions getUserActions)
        {
            _dbContext = dbContext;
            _logger = logger;
            _sendEmail = sendEmail;
            _blobImageService = blobImageService;
            _getUserActions = getUserActions;
        }

        public async Task<UpdateUserDTO> GetUserDataAsync()
        {
            var userId = _getUserActions.GetUserId();
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
            var findUser = await _getUserActions.GetUserByEmailAsync(model.Email);
            var findNickname = await _getUserActions.GetUserByNicknameAsync(model.Nickname);
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
            var userId = _getUserActions.GetUserId();
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found");
            }

            var updateStrategies = new List<IUserUpdateStrategy>
            {
                new EmailUpdateStrategy(_sendEmail, _getUserActions),
                new NicknameUpdateStrategy(_getUserActions),
                new ProfilePictureUpdateStrategy(_blobImageService)
            };

            foreach (var strategy in updateStrategies)
            {
                await strategy.UpdateUserAsync(model, user);
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
