using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyMonkey.Application.Services;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Communication.Response;

namespace MoneyMonkey.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(UserResponseList), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers()
    {
        var response = await _userService.GetAllUsers();

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateUser([FromBody] UserRequest request)
    {
        var response = await _userService.CreateUser(request);

        return Ok(response);
    }
}
