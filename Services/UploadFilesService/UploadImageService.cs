using Azure.Storage.Blobs;
using BackendChat.Services.Interfaces;

namespace BackendChat.Services.UploadFilesServices
{
    public class UploadImageService : IUploadImageService
    {
        private readonly IConfiguration _configuration;
        private readonly string[] permittedExtensionsToProfile = { ".jpg", ".jpeg", ".png" };
        private readonly string[] permittedMimeTypesToProfile = { "image/jpeg", "image/png" };
        public UploadImageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadProfileImageAsync(IFormFile imageStream)
        {
            if (imageStream == null || imageStream.Length == 0)
            {
                //No image uploaded, return the URL of the default image
                return GetDefaultImageUrl();
            }

            //Validate file extension
            var extension = Path.GetExtension(imageStream.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(imageStream.ContentType) || !permittedMimeTypesToProfile.Contains(imageStream.ContentType))
            {
                if (Array.IndexOf(permittedExtensionsToProfile, extension) < 0)
                {
                    throw new InvalidOperationException("Invalid file extension. Only .jpg, .jpeg, and .png are allowed.");
                }
            }

            //Validate image size (200 KB = 204800 bytes)
            if (imageStream.Length > 204800)
            {
                throw new InvalidOperationException("The image size exceeds the limit of 200 KB.");
            }

            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["AzureBlob:ConnectionString"]);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_configuration["AzureBlob:ContainerName"]);
            await containerClient.CreateIfNotExistsAsync();
            string blobName = "image" + Guid.NewGuid().ToString();
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            using (var stream = imageStream.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
        }

        public string GetDefaultImageUrl()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["AzureBlob:ConnectionString"]);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_configuration["AzureBlob:ContainerName"]);
            BlobClient defaultBlobClient = containerClient.GetBlobClient(_configuration["AzureBlob:DefaultImage"]);
            return defaultBlobClient.Uri.ToString();
        }
    }
}
