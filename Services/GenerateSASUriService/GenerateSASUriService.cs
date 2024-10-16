using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using BackendChat.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace BackendChat.Services.GenerateSASUriService
{
    public class GenerateSASUriService : IGenerateSASUriService, IGetBlobActionsService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName1;
        private readonly string _containerName2;

        public GenerateSASUriService(string connectionString, string containerName1, string containerName2)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }

            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName1 = containerName1 ?? throw new ArgumentNullException(nameof(containerName1));
            _containerName2 = containerName2 ?? throw new ArgumentNullException(nameof(containerName2));
        }

        public string GenerateBlobSasUri(TimeSpan duration, bool useContainer, string blobName)
        {
            string containerName = SelectContainerName(useContainer);
            var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
            if (blobClient == null)
            {
                throw new ArgumentNullException(nameof(blobClient));
            }
            // Define SAS permissions and expiry time
            var sasBuilder = new BlobSasBuilder()
            {
                
                BlobContainerName = containerName,
                BlobName = blobClient.Name,
                Resource = "b", // "b" stands for blob
                ExpiresOn = DateTimeOffset.UtcNow.Add(duration) // SAS duration
            };

            // Set permissions to read
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Generate the SAS URI
            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        public async Task<string> RegenerateSasUriAsync(string blobName, bool useContainer)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));
            }

            string containerName = SelectContainerName(useContainer);
            var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                // Create new SAS URI with readonly permissions
                var sasUri = GenerateBlobSasUri(TimeSpan.FromHours(1), useContainer, blobName);
                return sasUri;
            }

            throw new InvalidOperationException($"Blob '{blobName}' not found in container '{containerName}'.");
        }

        public async Task<string> UploadAndGenerateSasUriAsync(string blobName, Stream fileStream, TimeSpan sasDuration, bool useContainer)
        {
            string containerName = SelectContainerName(useContainer);
            var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
            await blobClient.UploadAsync(fileStream, overwrite: true);

            return GenerateBlobSasUri(sasDuration, useContainer, blobName);

        }

        public string GetDefaultImageUrl(string blobName ,bool useContainer)
        {
            string containerName = SelectContainerName(useContainer);
            var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName).Uri.ToString();

            return blobClient;
        }

        private string SelectContainerName(bool useContainer)
        {
            return useContainer ? _containerName1 : _containerName2;
        }
    }
}
