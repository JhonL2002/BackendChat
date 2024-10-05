using BackendChat.DTOs;
using BackendChat.Services.Interfaces;

namespace BackendChat.Services.SendEmail
{
    public class SendEmailService : ISendEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IMailJet _mailJet;
        public SendEmailService(IConfiguration configuration, IMailJet mailJet)
        {
            _configuration = configuration;
            _mailJet = mailJet;
        }
        public async Task SendConfirmationEmailAsync(RegisterDTO user)
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"];
            var encodedToken = Uri.EscapeDataString(user.EmailConfirmationToken);
            var confirmationLink = $"{baseUrl}/confirm-email?userNickname={user.Nickname}&token={encodedToken}";
            string message = $"Please, confirm your account here {confirmationLink}";
            await _mailJet.SendEmailAsync(_configuration["EmailCredentials:FromEmail"]!, user.Email, "Confirm your email", message);

        }
    }
}
