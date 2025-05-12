using MySite.Domain.Models;

namespace MySite.DataAccess.Services;

public interface IJwtProvider
{
    string GenerateToken(User user);
}