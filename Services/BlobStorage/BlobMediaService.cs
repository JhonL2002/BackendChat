using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace BackendChat.Services.BlobStorage
{
    public class BlobMediaService
    {
        private readonly IConfiguration _configuration;
        private readonly string[] permittedExtensionsToChat = { ".jpg", ".jpeg", ".png", ".mp4", ".mp3", ".wav", ".pdf", ".docx", ".xlsx", ".zip" };

        public BlobMediaService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadMediaAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("Please add a resource!");
            }

            //Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(file.ContentType))
            {
                if (Array.IndexOf(permittedExtensionsToChat, extension) < 0)
                {
                    throw new InvalidOperationException("Invalid file extension");
                }
            }

            //Validate file size based on type (200 KB = 204800 default bytes)
            long maxSize = GetMaxFileSize(extension);
            if (file.Length > maxSize)
            {
                throw new InvalidOperationException("The file size exceeds the limit of 200 KB.");
            }

            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["AzureBlob:ConnectionString"]);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_configuration["AzureBlob:ContainerName"]);
            await containerClient.CreateIfNotExistsAsync();
            string blobName = $"{Guid.NewGuid()}{extension}";

            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            
            //Upload file to blob
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            //Generate SAS URI with read permissions
            var sasUri = GenerateBlobSasUri(blobClient, TimeSpan.FromHours(1));

            return sasUri.ToString();
        }

        //New method to regenerate SAS URL
        public async Task<string> RegenerateSasUri(string blobName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["AzureBlob:ConnectionString"]);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_configuration["AzureBlob:ContainerName"]);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                //Create new SAS URI with readonly permissions
                var sasUri = GenerateBlobSasUri(blobClient, TimeSpan.FromHours(1));
                return sasUri;
            }
            throw new InvalidOperationException("Blob not found.");
        }

        private string GenerateBlobSasUri(BlobClient blobClient, TimeSpan duration)
        {
            //Define SAS permissions and expiry time
            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b", // "b" stands for blob
                ExpiresOn = DateTimeOffset.UtcNow.Add(duration) // SAS duration
                
            };

            //Set permissions to read
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            //Generate the SAS URI
            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }

        private long GetMaxFileSize(string extension)
        {
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                    return 204800; //200KB
                case ".mp4":
                    return 10485760; //10MB
                case ".mp3":
                case ".wav":
                    return 5120000; //5MB
                case ".pdf":
                case ".docx":
                case ".xlsx":
                    return 2048000; //2MB
                case ".zip":
                    return 5242880; //5MB
                default:
                    return 204800; //Default 200KB
            }
        }
    }
}
