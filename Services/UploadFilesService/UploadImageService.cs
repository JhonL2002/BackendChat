using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using BackendChat.Constants;
using BackendChat.Services.Interfaces;

namespace BackendChat.Services.UploadFilesServices
{
    public class UploadImageService<T> : IUploadMediaService<T>
    {
        private readonly IConfiguration _configuration;
        private readonly IGenerateSASUriService _generateSASUriService;
        private readonly IGetBlobActionsService _getBlobActionsService;
        private readonly string[] permittedExtensionsToProfile = { ".jpg", ".jpeg", ".png" };
        private readonly string[] permittedMimeTypesToProfile = { "image/jpeg", "image/png" };
        public UploadImageService(IConfiguration configuration, IGenerateSASUriService generateSASUriService, IGetBlobActionsService getBlobActionsService)
        {
            _configuration = configuration;
            _generateSASUriService = generateSASUriService;
            _getBlobActionsService = getBlobActionsService;
        }

        public async Task<string> UploadMediaAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                //No image uploaded, return the URL of the default image
                return _getBlobActionsService.GetDefaultImageUrl(_configuration["AzureBlob:DefaultImage"]!, BlobContainerEnum.Container2);
            }

            //Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(file.ContentType) || !permittedMimeTypesToProfile.Contains(file.ContentType))
            {
                if (Array.IndexOf(permittedExtensionsToProfile, extension) < 0)
                {
                    throw new InvalidOperationException("Invalid file extension. Only .jpg, .jpeg, and .png are allowed.");
                }
            }

            //Validate image size (200 KB = 204800 bytes)
            if (file.Length > 204800)
            {
                throw new InvalidOperationException("The image size exceeds the limit of 200 KB.");
            }

            string blobName = "image" + Guid.NewGuid().ToString();

            await using var stream = file.OpenReadStream();

            var sasUri = await _generateSASUriService.UploadAndGenerateSasUriAsync(blobName, stream, TimeSpan.FromHours(1), BlobContainerEnum.Container2);

            return sasUri;
        }
    }
}
