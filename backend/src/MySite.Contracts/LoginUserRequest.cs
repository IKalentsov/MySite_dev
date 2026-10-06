using System.ComponentModel.DataAnnotations;

namespace MySite.Contracts;

/// <summary>Credentials for signing in. Not part of the public site yet.</summary>
/// <param name="Email">The address the account was registered with.</param>
/// <param name="Password">The account's password.</param>
public record LoginUserRequest(
    [Required] string Email,
    [Required] string Password);
