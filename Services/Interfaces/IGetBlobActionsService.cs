namespace BackendChat.Services.Interfaces
{
    public interface IGetBlobActionsService
    {
        string GetDefaultImageUrl(string blobName, bool useContainer);
    }
}
