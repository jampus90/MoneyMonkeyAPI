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
public class TransactionController : ControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(TransactionResponseList), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions()
    {
        var response = await _transactionService.GetAllTransactions(GetUserId());

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequest request)
    {
        var response = await _transactionService.CreateTransaction(GetUserId(), request);
        if (response is null)
        {
            return BadRequest(new { message = "Categoria inválida para este usuário." });
        }

        return Ok(response);
    }

    private long GetUserId()
    {
        return long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
