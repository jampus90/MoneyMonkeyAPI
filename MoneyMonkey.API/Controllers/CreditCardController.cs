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
public class CreditCardController : ControllerBase
{
    private readonly CreditCardService _creditCardService;
    private readonly CreditCardPurchaseService _creditCardPurchaseService;

    public CreditCardController(CreditCardService creditCardService, CreditCardPurchaseService creditCardPurchaseService)
    {
        _creditCardService = creditCardService;
        _creditCardPurchaseService = creditCardPurchaseService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CreditCardResponseList), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCreditCards()
    {
        var response = await _creditCardService.GetAllCreditCards(GetUserId());

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreditCardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCreditCard([FromBody] CreditCardRequest request)
    {
        var response = await _creditCardService.CreateCreditCard(GetUserId(), request);

        return Ok(response);
    }

    [HttpPost("{creditCardId}/purchases")]
    [ProducesResponseType(typeof(CreditCardInstallmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePurchase(int creditCardId, [FromBody] CreditCardPurchaseRequest request)
    {
        var response = await _creditCardPurchaseService.CreatePurchase(GetUserId(), creditCardId, request);
        if (response is null)
        {
            return BadRequest(new { message = "Cartão ou categoria inválidos para este usuário." });
        }

        return Ok(response);
    }

    [HttpGet("{creditCardId}/fatura")]
    [ProducesResponseType(typeof(CreditCardInvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInvoice(int creditCardId, [FromQuery] int? month, [FromQuery] int? year)
    {
        var response = await _creditCardPurchaseService.GetInvoice(GetUserId(), creditCardId, month, year);
        if (response is null)
        {
            return BadRequest(new { message = "Cartão inválido para este usuário." });
        }

        return Ok(response);
    }

    private long GetUserId()
    {
        return long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
