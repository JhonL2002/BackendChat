namespace BackendChat.Services.Interfaces
{
    public interface IBlobImageService
    {
        Task<string> UploadProfileImageAsync(IFormFile imageStream);
        string GetDefaultImageUrl();
    }
}
