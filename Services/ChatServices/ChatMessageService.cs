using BackendChat.Data;
using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Responses;
using BackendChat.Services.BlobStorage;
using BackendChat.Services.EmailSender;
using Mailjet.Client.Resources;
using Microsoft.EntityFrameworkCore;

namespace BackendChat.Services.ChatServices
{
    public class ChatMessageService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<ChatMessageService> _logger;
        private readonly BlobMediaService _blobMediaService;
        public ChatMessageService(
            AppDbContext appDbContext,
            ILogger<ChatMessageService> logger,
            BlobMediaService blobMediaService
            )
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _blobMediaService = blobMediaService;

        }
        public async Task RegisterMessageAsync(ChatMessageDTO model)
        {
            if (model.Timestamp == null)
            {
                model.Timestamp = DateTime.UtcNow;
            }
            if (model.File != null)
            {
                //Upload an image and get the url
                var imageUrl = await _blobMediaService.UploadMediaAsync(model.File);
                model.MediaUrl = imageUrl;
            }
            else
            {
                model.MediaUrl = null;
            }
            var newMessage = new ChatMessage
            {
                UserId = model.UserId,
                ChatId = model.ChatId,
                Text = model.Text,
                MediaUrl = model.MediaUrl,
                Timestamp = DateTime.UtcNow
            };
            _appDbContext.ChatMessages.Add(newMessage);
            await _appDbContext.SaveChangesAsync();
            _logger.LogInformation("**** Success: Message saved successfully! ****");
        }

        public async Task<List<ChatMediaResponse>> GetMessagesWithUpdatedSaSUrlsAsync(int chatId)
        {
            //Verify if chatId exists
            var chatExists = await _appDbContext.Chats.AnyAsync(c => c.ChatId == chatId);
            if (!chatExists)
            {
                throw new ArgumentException("The specified chatId does not exist");
            }

            var messages = await _appDbContext.ChatMessages
                .Where(m => m.ChatId == chatId)
                .Select(m => new ChatMediaResponse
                {
                    MessageId = m.MessageId,
                    UserId = m.UserId,
                    UserName = m.User.FirstName + " " + m.User.LastName,
                    ChatId = m.ChatId,
                    Text = m.Text,
                    MediaUrl = m.MediaUrl,
                    Timestamp = m.Timestamp ?? DateTime.UtcNow
                }).ToListAsync();

            var updatedMessages = new List<ChatMediaResponse>();

            foreach (var message in messages)
            {
                if (!string.IsNullOrEmpty(message.MediaUrl))
                {
                    //Get the name of blob from Url
                    var blobName = ExtractBlobNameFromUrl(message.MediaUrl);

                    //Regenerate SAS Url for blob
                    var newSasUrl = await _blobMediaService.RegenerateSasUri(blobName);

                    message.MediaUrl = newSasUrl;
                }

                updatedMessages.Add(message);
            }
            return updatedMessages;
        }

        private string ExtractBlobNameFromUrl(string url)
        {
            return new Uri(url).Segments.Last();
        }
    }
}
