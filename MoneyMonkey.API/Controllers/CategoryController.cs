using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyMonkey.Application.Services;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Communication.Response;

namespace MoneyMonkey.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoryController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CategoryResponseList), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var response = await _categoryService.GetAllCategories(GetUserId());

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest request)
    {
        var response = await _categoryService.CreateCategory(GetUserId(), request);

        return Ok(response);
    }

    private long GetUserId()
    {
        return long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
