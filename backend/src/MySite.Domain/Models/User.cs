using MySite.Domain.Common.Enums;
using MySite.Domain.Interfaces;

namespace MySite.Domain.Models;

public class User : IAuditable
{
    public const int MaxTitleLength = 250;

    private User(Guid id, DateTime created, DateTime modified, string login, string firstName,
        string lastName, string email, string passwordHash, int right, string profileImage)
    {
        Id = id;
        Created = created;
        Modified = modified;
        Login = login;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Right = right;
        ProfileImage = profileImage;
    }

    public Guid Id { get; }
    public DateTime Created { get; }
    public DateTime Modified { get; }

    public string Login { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string PasswordHash { get; }
    public int Right { get; }

    public string ProfileImage { get; }

    public static (User user, string Error) Create(Guid id, DateTime created, DateTime modified, string login, string firstName,
        string lastName, string email, string passwordHash, int right, string profileImage)
    {
        var err = string.Empty;

        if (string.IsNullOrEmpty(login) || login.Length > MaxTitleLength)
        {
            err = "Превышена допустимая длина логина";
        }

        var user = new User (id, created, modified, login, firstName, lastName, email, passwordHash, right, profileImage);

        return (user, err);
    }
}
