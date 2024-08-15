using Azure.Storage.Blobs;

namespace BackendChat.Services.BlobStorage
{
    public class BlobImageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BlobImageService> _logger;
        private readonly string[] permittedExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly string[] permittedMimeTypes = { "image/jpeg", "image/png" };
        public BlobImageService(IConfiguration configuration, ILogger<BlobImageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
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
            if (string.IsNullOrEmpty(imageStream.ContentType) || !permittedMimeTypes.Contains(imageStream.ContentType))
            {
                if (Array.IndexOf(permittedExtensions, extension) < 0)
                {
                    throw new InvalidOperationException("Invalid file extension. Only .jpg, .jpeg, and .png are allowed.");
                }
            }

            //Validate file MIME type
            /*if (!permittedMimeTypes.Contains(imageStream.ContentType))
            {
                _logger.LogInformation($"File extension: {extension}");
                _logger.LogInformation($"MIME type: {imageStream.ContentType}");
                throw new InvalidOperationException("Invalid file type. Only JPEG and PNG are allowed.");
            }*/

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

        public async Task<Stream> GetProfileImage(string userId)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["AzureBlob:ConnectionString"]);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_configuration["AzureBlob:ContainerName"]);

            string blobName = $"{userId}.jpg";
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                return await blobClient.OpenReadAsync();
            }
            else
            {
                //Return the default image if the user does not have a profile image
                BlobClient defaultBlobClient = containerClient.GetBlobClient(_configuration["AzureBlob:DefaultImage"]);
                return await defaultBlobClient.OpenReadAsync();
            }
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
