using BackendChat.DTOs;
using BackendChat.Helpers.Interfaces;
using BackendChat.Repositories.Interfaces;

namespace BackendChat.Repositories.UserAccount
{
    public class LoginRepository : ILoginRepository
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ILoginRepository> _logger;
        private readonly IAdminTokenCode _adminTokenCode;
        public LoginRepository(IUserRepository userRepository, ILogger<ILoginRepository> logger, IAdminTokenCode adminTokenCode)
        {
            _userRepository = userRepository;
            _logger = logger;
            _adminTokenCode = adminTokenCode;
        }
        public async Task<string?> LoginAsync(LoginDTO model)
        {
            var finduser = await _userRepository.GetUser(model.Email);
            if (finduser == null)
            {
                _logger.LogError("**** User not found! ****");
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, finduser.Password))
            {
                _logger.LogError("**** Email/Password invalid! ****");
                return null;
            }

            if (!finduser.EmailConfirmed)
            {
                _logger.LogError("**** Email not confirmed! ****");
                return null;
            }

            string jwtToken = _adminTokenCode.GenerateToken(finduser);
            _logger.LogInformation($"**** Login Succesfully **** {jwtToken}");
            return jwtToken;
        }
    }
}
