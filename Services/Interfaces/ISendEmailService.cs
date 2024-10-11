using BackendChat.DTOs;

namespace BackendChat.Services.Interfaces
{
    public interface ISendEmailService
    {
        Task SendConfirmationEmailAsync<T>(T model);
    }
}
