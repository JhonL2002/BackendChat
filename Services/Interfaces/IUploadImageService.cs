namespace BackendChat.Services.Interfaces
{
    public interface IUploadImageService
    {
        Task<string> UploadProfileImageAsync(IFormFile imageStream);
        string GetDefaultImageUrl();
    }
}
