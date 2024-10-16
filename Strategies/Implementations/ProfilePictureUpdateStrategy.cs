using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Repositories.Interfaces;
using BackendChat.Services.Interfaces;
using BackendChat.Strategies.Interfaces;

namespace BackendChat.Strategies.Implementations
{
    public class ProfilePictureUpdateStrategy : IUserUpdateStrategy
    {
        private readonly IUploadMediaService<IUserRepository> _uploadImageService;
        public ProfilePictureUpdateStrategy(IUploadMediaService<IUserRepository> uploadImageService)
        {
            _uploadImageService = uploadImageService;
        }
        public async Task UpdateUserAsync(UpdateUserDTO model, AppUser user)
        {
            if (model.ProfilePicture != null)
            {
                var profilePictureUrl = await _uploadImageService.UploadMediaAsync(model.ProfilePicture);
                user.ProfilePictureUrl = profilePictureUrl;
            }
        }
    }
}
