using MySite.Domain.Common.Enums;

namespace MySite.DataAccess.Entities;
public class UserEntity
{
    public Guid Id { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Modified { get; set; } = DateTime.Now;

    public string Login { get; set; } = string.Empty;
    public string FirstName { get; set;} = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int Right { get; set; } = (int)UserRight.None;

    public string ProfileImage { get; set; } = string.Empty;
}
