using System.ComponentModel.DataAnnotations;

namespace MySite.Contracts;

/// <summary>Details for creating an account. Not part of the public site yet.</summary>
/// <param name="Login">The name the account signs in with.</param>
/// <param name="FirstName">The owner's first name.</param>
/// <param name="LastName">The owner's last name.</param>
/// <param name="Email">The owner's email address.</param>
/// <param name="Password">The password to set.</param>
/// <param name="ProfileImage">A link to the account's picture.</param>
public record RegisterUserRequest(
    [Required] string Login,
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string Email,
    [Required] string Password,
    [Required] string ProfileImage);
