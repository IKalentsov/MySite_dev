using Microsoft.AspNetCore.Mvc;
using MySite.Application.Services;
using MySite.Contracts;

namespace MySite.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UsersService _usersService;

    public UserController(UsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpPost("register")]
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
