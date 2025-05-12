using System.ComponentModel.DataAnnotations;

namespace MySite.Api.Contracts;

public record RegisterUserRequest(
    [Required] string Login,
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string Email, 
    [Required] string Password, 
    [Required] string ProfileImage);
