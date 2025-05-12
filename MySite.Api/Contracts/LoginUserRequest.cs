using System.ComponentModel.DataAnnotations;

namespace MySite.Api.Contracts;

public record LoginUserRequest(
    [Required] string Email,
    [Required] string Password);
