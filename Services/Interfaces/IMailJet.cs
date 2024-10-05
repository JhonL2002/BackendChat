namespace BackendChat.Services.Interfaces
{
    public interface IMailJet
    {
        Task SendEmailAsync(string fromEmail, string toEmail, string subject, string body);
    }
}
