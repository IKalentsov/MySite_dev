using Microsoft.AspNetCore.Mvc;
using MySite.Application.Services;
using MySite.Contracts;

namespace MySite.Web.Controllers;

/// <summary>
/// Account endpoints. Dormant: the first phase of the site is public, and authentication arrives
/// with the administration panel (see docs/adr/0003-public-site-first-phase.md).
/// </summary>
[Route("[controller]")]
[ApiController]
public sealed class UserController : ControllerBase
{
    private readonly UsersService _usersService;

    /// <summary>Creates the controller.</summary>
    /// <param name="usersService">The account use cases.</param>
    public UserController(UsersService usersService)
    {
        _usersService = usersService;
    }

    /// <summary>Registers an account. Not reachable in the first phase.</summary>
    /// <param name="request">The account to create.</param>
    /// <returns>An empty success response.</returns>
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

    /// <summary>Signs an account in. Not reachable in the first phase.</summary>
    /// <param name="request">The credentials to check.</param>
    /// <returns>The issued token.</returns>
    [HttpGet("login")]
    public async Task<IActionResult> Login(
        LoginUserRequest request)
    {
        var token = await _usersService.Login(request.Email, request.Password);

        return Ok(token);
    }
}
