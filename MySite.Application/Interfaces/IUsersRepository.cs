using MySite.Domain.Models;

namespace MySite.Application.Interfaces;
public interface IUsersRepository
{
    Task Add(User user);
    Task<User> GetByEmailAsync(string email);
}
