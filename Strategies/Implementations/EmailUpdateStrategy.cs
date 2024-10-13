using BackendChat.Data;
using BackendChat.DTOs;
using BackendChat.Helpers;
using BackendChat.Models;
using BackendChat.Repositories.AccountRepositories;
using BackendChat.Repositories.Interfaces;
using BackendChat.Services.Interfaces;
using BackendChat.Strategies.Interfaces;
using Mailjet.Client.Resources;

namespace BackendChat.Strategies.Implementations
{
    public class EmailUpdateStrategy : IUserUpdateStrategy
    {
        private readonly ISendEmailService _sendEmailService;
        private readonly IGetUserActions _getUserActions;

        public EmailUpdateStrategy(
            ISendEmailService sendEmailService,
            IGetUserActions getUserActions)
        {
            _sendEmailService = sendEmailService;
            _getUserActions = getUserActions;
        }
        public async Task UpdateUserAsync(UpdateUserDTO model, AppUser user)
        {
            //Verify if the current email is different
            if (user.Email != model.Email)
            {
                var findUser = await _getUserActions.GetUserByEmailAsync(model.Email);
                if (findUser != null && findUser.Id != _getUserActions.GetUserId())
                {
                    throw new InvalidOperationException("Email already in use by another user.");
                }
                //User needs to re-confirm email
                user.Email = model.Email;
                user.EmailConfirmed = false;

                //Generate a GUID Token to send, update this in model
                model.EmailConfirmationToken = GenerateGuidCode.GenerateGuidToken();
                user.EmailConfirmationToken = model.EmailConfirmationToken;
                await _sendEmailService.SendConfirmationEmailAsync(model);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Email sent succesfully to {model.Email}!");
            }
        }
    }
}
