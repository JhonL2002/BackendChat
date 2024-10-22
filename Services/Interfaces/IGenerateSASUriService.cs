using Azure.Storage.Blobs;
using BackendChat.Constants;

namespace BackendChat.Services.Interfaces
{
    public interface IGenerateSASUriService
    {
        Task<string> RegenerateSasUriAsync(string blobName, BlobContainerEnum container);
        string GenerateBlobSasUri(TimeSpan duration, BlobContainerEnum container, string blobName);
        Task<string> UploadAndGenerateSasUriAsync(string blobName, Stream fileStream, TimeSpan sasDuration, BlobContainerEnum container);
    }
}
