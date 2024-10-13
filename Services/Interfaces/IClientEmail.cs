namespace BackendChat.Services.Interfaces
{
    public interface IClientEmail
    {
        Task SendEmailAsync(string fromEmail, string toEmail, string subject, string body);
    }
}
