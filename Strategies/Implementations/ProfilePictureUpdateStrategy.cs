﻿using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Services.Interfaces;
using BackendChat.Strategies.Interfaces;

namespace BackendChat.Strategies.Implementations
{
    public class ProfilePictureUpdateStrategy : IUserUpdateStrategy
    {
        private readonly IUploadImageService _uploadImageService;
        public ProfilePictureUpdateStrategy(IUploadImageService uploadImageService)
        {
            _uploadImageService = uploadImageService;
        }
        public async Task UpdateUserAsync(UpdateUserDTO model, AppUser user)
        {
            if (model.ProfilePicture != null)
            {
                var profilePictureUrl = await _uploadImageService.UploadProfileImageAsync(model.ProfilePicture);
                user.ProfilePictureUrl = profilePictureUrl;
            }
        }
    }
}