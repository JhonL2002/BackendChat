using Azure.Storage.Blobs;

namespace BackendChat.Services.Interfaces
{
    public interface IGenerateSASUriService
    {
        Task<string> RegenerateSasUriAsync(string blobName, bool useContainer);
        string GenerateBlobSasUri(TimeSpan duration, bool useContainer, string blobName);
        Task<string> UploadAndGenerateSasUriAsync(string blobName, Stream fileStream, TimeSpan sasDuration, bool useContainer);
    }
}
