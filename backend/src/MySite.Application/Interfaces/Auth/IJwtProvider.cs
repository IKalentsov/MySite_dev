using MySite.Domain.Models;

namespace MySite.Application.Interfaces.Auth;

public interface IJwtProvider
{
    string GenerateToken(User user);
}