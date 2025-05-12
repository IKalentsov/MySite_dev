using Microsoft.AspNetCore.Mvc;
using MySite.Api.Contracts;
using MySite.Application.Services;

namespace MySite.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UsersService _usersService;

    [ HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterUserRequest request)
    {
        await _usersService.Register(
            request.Login,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.ProfileImage);

        return Ok();
    }

    [HttpGet("login")]
    public async Task<IActionResult> Login(
        LoginUserRequest request)
    {
        var token = await _usersService.Login(request.Email, request.Password);

        return Ok(token);
    }
}
