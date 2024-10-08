using Azure.Storage.Blobs;

namespace BackendChat.Services.Interfaces
{
    public interface IUploadMediaService
    {
        Task<string> UploadMediaAsync(IFormFile file);
        Task<string> RegenerateSasUriAsync(string blobName);
        string GenerateBlobSasUri(BlobClient blobClient, TimeSpan duration);
        long GetMaxFileSize(string extension);
    }
}
