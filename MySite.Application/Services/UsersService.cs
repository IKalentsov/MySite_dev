using MySite.Application.Interfaces;
using MySite.Application.Interfaces.Auth;
using MySite.DataAccess.Services;
using MySite.Domain.Common.Enums;
using MySite.Domain.Models;

namespace MySite.Application.Services;

public class UsersService
{
    private readonly IJwtProvider _jwtProvider;
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UsersService(
        IJwtProvider jwtProvider,
        IUsersRepository usersRepository,
        IPasswordHasher passwordHasher)
    {
        _jwtProvider = jwtProvider;
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task Register(string login, string firstName, string lastName,
        string email, string password, string profileImage)
    {
        var hashedPassword = _passwordHasher.Generate(password);

        var user = User.Create(
            Guid.NewGuid(),
            DateTime.Now,
            DateTime.Now,
            login,
            firstName,
            lastName,
            email,
            hashedPassword,
            (int) UserRight.None,
            string.Empty);

        await _usersRepository.Add(user.user);
    }

    public async Task<string> Login(string email, string password)
    {
        var user = await _usersRepository.GetByEmailAsync(email);

        var result = _passwordHasher.Verify(password, user.PasswordHash);

        if (!result)
        {
            throw new Exception("Failed to login");
        }

        var token = _jwtProvider.GenerateToken(user);

        return token;
    }
}
