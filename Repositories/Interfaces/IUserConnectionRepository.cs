namespace BackendChat.Repositories.Interfaces
{
    public interface IUserConnectionRepository
    {
        Task<IEnumerable<string>> GetUserConnectionsAsync(string userId);
    }
}
