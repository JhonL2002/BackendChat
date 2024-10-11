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
        public async Task SendConfirmationEmailAsync<T>(T model)
        {
            //Verify that model does not null
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Model cannot be null");
            }

            //Use reflection to get required properties
            var emailProperty = typeof(T).GetProperty("Email")?.GetValue(model)?.ToString();
            var nicknameProperty = typeof(T).GetProperty("Nickname")?.GetValue(model)?.ToString();
            var tokenProperty = typeof(T).GetProperty("EmailConfirmationToken")?.GetValue(model)?.ToString();

            //Verify that necessary properties does not be null
            if (string.IsNullOrEmpty(emailProperty) || string.IsNullOrEmpty(nicknameProperty) || string.IsNullOrEmpty(tokenProperty))
            {
                throw new InvalidOperationException("Model must contain properties such as Email, Nickname and EmailConfirmationToken.");
            }

            var baseUrl = _configuration["AppSettings:BaseUrl"];
            var encodedToken = Uri.EscapeDataString(tokenProperty);
            var confirmationLink = $"{baseUrl}/confirm-email?userNickname={nicknameProperty}&token={encodedToken}";
            string message = $"Please, confirm your account here {confirmationLink}";

            //Send email
            await _mailJet.SendEmailAsync(_configuration["EmailCredentials:FromEmail"]!, emailProperty, "Confirm your email", message);

        }
    }
}
