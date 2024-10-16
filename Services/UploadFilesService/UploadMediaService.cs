using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using BackendChat.Services.Interfaces;

namespace BackendChat.Services.UploadFilesServices
{
    public class UploadMediaService<T> : IUploadMediaService<T>
    {
        private readonly IGenerateSASUriService _generateSASUriService;
        private readonly string[] permittedExtensionsToChat = { ".jpg", ".jpeg", ".png", ".mp4", ".mp3", ".wav", ".pdf", ".docx", ".xlsx", ".zip" };

        public UploadMediaService(IGenerateSASUriService generateSASUriService)
        {
            _generateSASUriService = generateSASUriService;
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

            string blobName = $"{Guid.NewGuid()}{extension}";

            //Upload file to blob
            await using var stream = file.OpenReadStream();

            //Generate SAS URI with read permissions
            var sasUri = await _generateSASUriService.UploadAndGenerateSasUriAsync(blobName, stream, TimeSpan.FromHours(1), true);

            return sasUri;
        }

        public long GetMaxFileSize(string extension)
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
