using BackendChat.Services.Interfaces;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;

namespace BackendChat.Services.MailJet
{
    public class MailJet : IMailJet
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IMailJet> _logger;

        public MailJet(IConfiguration configuration, ILogger<IMailJet> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string fromEmail, string toEmail, string subject, string body)
        {
            MailjetClient client = new MailjetClient(_configuration["EmailCredentials:ApiKey"], _configuration["EmailCredentials:SecretKey"]);
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource
            }
            .Property(Send.FromEmail, fromEmail)
            .Property(Send.FromName, "Application Team")
            .Property(Send.Subject, subject)
            .Property(Send.HtmlPart, body)
            .Property(Send.Recipients, new JArray
            {
                new JObject
                {
                    { "Email", toEmail }
                }
            });

            MailjetResponse response = await client.PostAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Email sent succesfully to {toEmail}");
            }
            else
            {
                _logger.LogError($"Failed to send email. StatusCode {response.StatusCode}, Error: {response.GetErrorMessage()}");
            }
        }
    }
}
