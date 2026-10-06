using System.ComponentModel.DataAnnotations;

namespace MySite.Contracts;

public record LoginUserRequest(
    [Required] string Email,
    [Required] string Password);
