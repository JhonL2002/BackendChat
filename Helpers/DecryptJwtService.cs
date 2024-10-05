using BackendChat.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BackendChat.Helpers
{
    public static class DecryptJwtService
    {
        public static CustomUserClaims DecryptToken(string jwtToken)
        {
            try
            {
                if (string.IsNullOrEmpty(jwtToken))
                    return new CustomUserClaims();
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwtToken);

                var name = token.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Name);
                var email = token.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Email);
                var identifier = token.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier);

                return new CustomUserClaims(name!.Value, email!.Value, identifier!.Value);
            }
            catch
            {
                return null!;
            }
        }
    }
}
