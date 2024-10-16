using Azure.Storage.Blobs;

namespace BackendChat.Services.Interfaces
{
    public interface IUploadMediaService<T>
    {
        Task<string> UploadMediaAsync(IFormFile file);
    }
}
