using Graduation.Entities;

namespace Graduation.Services.Jwt
{
    public interface IJwtService
    {
        string GenerateJwtToken(User user);
    }
}
