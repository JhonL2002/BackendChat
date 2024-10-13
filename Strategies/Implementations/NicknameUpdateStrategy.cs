using BackendChat.Data;
using BackendChat.DTOs;
using BackendChat.Models;
using BackendChat.Repositories.AccountRepositories;
using BackendChat.Repositories.Interfaces;
using BackendChat.Strategies.Interfaces;

namespace BackendChat.Strategies.Implementations
{
    public class NicknameUpdateStrategy : IUserUpdateStrategy
    {
        private readonly IGetUserActions _getUserActions;
        public NicknameUpdateStrategy(IGetUserActions getUserActions)
        {
            _getUserActions = getUserActions;
        }
        public async Task UpdateUserAsync(UpdateUserDTO model, AppUser user)
        {
            if (user.Nickname != model.Nickname)
            {
                var findNickname = await _getUserActions.GetUserByNicknameAsync(model.Nickname);
                if (findNickname != null)
                {
                    throw new InvalidOperationException("Nickname already in use by another user");
                }
                user.Nickname = model.Nickname;
            }
        }
    }
}
