using BackendChat.Data;
using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Services.UploadFilesServices;
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
        private readonly UploadImageService _blobService;
        public AccountService(AppDbContext appDbContext,
            IConfiguration configuration,
            ILogger<AccountService> logger,
            UploadImageService blobService
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
    }
}