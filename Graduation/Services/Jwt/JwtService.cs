using Graduation.Entities;
using Graduation.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Graduation.Services.Jwt
{
    public class JwtService: IJwtService
    {
        private readonly JwtSettings _Settings;

        public JwtService(IOptions<JwtSettings> settings)
        {
            _Settings = settings.Value;
        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Settings.Key));
            var creadentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                signingCredentials: creadentials,
                claims: ExtractUserClaims(user));

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private Claim[] ExtractUserClaims(User user)
            => new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.GivenName, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
    }
}
