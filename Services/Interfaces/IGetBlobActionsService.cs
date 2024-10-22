using BackendChat.Constants;

namespace BackendChat.Services.Interfaces
{
    public interface IGetBlobActionsService
    {
        string GetDefaultImageUrl(string blobName, BlobContainerEnum container);
    }
}
