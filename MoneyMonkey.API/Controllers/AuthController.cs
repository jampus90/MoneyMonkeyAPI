using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyMonkey.Application.Services;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Communication.Response;

namespace MoneyMonkey.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _userService.Login(request);
        if (response is null)
        {
            return Unauthorized(new { message = "Usuário ou senha inválidos." });
        }

        return Ok(response);
    }
}
