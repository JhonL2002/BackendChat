namespace BackendChat.Repositories.Interfaces
{
    public interface IUserConnectionContext
    {
        Task<IEnumerable<string>> GetUserConnectionsAsync(string userId);
    }
}
