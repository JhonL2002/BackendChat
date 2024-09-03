﻿using BackendChat.Data;
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
            //Create a response to deserialize in client
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

        public async Task<List<ChatMediaResponse>> GetMessagesWithUpdatedSaSUrlsAsync(int chatId, int? lastMessageId = null, int pageSize = 6)
        {
            //Verify if chatId exists
            var chatExists = await _appDbContext.Chats.AnyAsync(c => c.ChatId == chatId);
            if (!chatExists)
            {
                throw new ArgumentException("The specified chatId does not exist");
            }

            //Get the timestamp from last message if lastMessageId exists
            DateTime? lastMessageTimestamp = null;
            if (lastMessageId.HasValue)
            {
                lastMessageTimestamp = await _appDbContext.ChatMessages
                    .Where(m => m.MessageId == lastMessageId.Value)
                    .Select(m => m.Timestamp)
                    .FirstOrDefaultAsync();
            }

            //Build query messages
            var messagesQuery = _appDbContext.ChatMessages
                .Where(m => m.ChatId == chatId)
                .AsQueryable();

            //Apply timestamp filter if lastMessageTimestamp is provided
            if (lastMessageTimestamp.HasValue)
            {
                messagesQuery = messagesQuery.Where(m => m.Timestamp < lastMessageTimestamp.Value);
            }

            //Order by Descending timestamp and select necesary fields
            var messages = await messagesQuery
                .OrderByDescending(m => m.Timestamp)
                .Select(m => new
                {
                    m.MessageId,
                    m.UserId,
                    UserName = m.User.FirstName + " " + m.User.LastName,
                    m.ChatId,
                    m.Text,
                    m.MediaUrl,
                    m.Timestamp
                })
                .Take(pageSize)
                .ToListAsync();

            //Use tasks to update URLs in parallel
            var updateTasks = messages.Select(async message =>
            {
                var response = new ChatMediaResponse
                {
                    MessageId = message.MessageId,
                    UserId = message.UserId,
                    UserName = message.UserName,
                    ChatId = message.ChatId,
                    Text = message.Text,
                    Timestamp = message.Timestamp ?? DateTime.UtcNow
                };

                if (!string.IsNullOrEmpty(message.MediaUrl))
                {
                    var blobName = ExtractBlobNameFromUrl(message.MediaUrl);

                    var newSasUrl = await _blobMediaService.RegenerateSasUri(blobName);
                    response.MediaUrl = newSasUrl;
                }
                else
                {
                    response.MessageId = message.MessageId;
                }

                return response;
            });

            var updatedMessages = (await Task.WhenAll(updateTasks)).ToList();
            return updatedMessages;
        }

        //Extract the name of file to re-build a new SAS Url
        private string ExtractBlobNameFromUrl(string url)
        {
            return new Uri(url).Segments.Last();
        }
    }
}
