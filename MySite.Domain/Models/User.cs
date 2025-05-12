using MySite.Domain.Common.Enums;
using MySite.Domain.Interfaces;

namespace MySite.Domain.Models;

public class User : IAuditable
{
    public const int MAX_TITLE_LENGTH = 250;

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
    public DateTime Created { get; } = DateTime.Now;
    public DateTime Modified { get; } = DateTime.Now;

    public string Login { get; } = string.Empty;
    public string FirstName { get; } = string.Empty;
    public string LastName { get; } = string.Empty;
    public string Email { get; } = string.Empty;
    public string PasswordHash { get;  } = string.Empty;
    public int Right { get; } = (int)UserRight.None;

    public string ProfileImage { get; } = string.Empty;

    public static (User user, string Error) Create(Guid id, DateTime created, DateTime modified, string login, string firstName,
        string lastName, string email, string passwordHash, int right, string profileImage)
    {
        var err = string.Empty;

        if (string.IsNullOrEmpty(login) || login.Length > MAX_TITLE_LENGTH)
        {
            err = "Превышена допустимая длина логина";
        }

        var user = new User (id, created, modified, login, firstName, lastName, email, passwordHash, right, profileImage);

        return (user, err);
    }
}
