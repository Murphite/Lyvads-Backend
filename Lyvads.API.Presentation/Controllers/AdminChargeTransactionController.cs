using Lyvads.Application.Dtos;
using Lyvads.Application.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AdminChargeTransactionController : ControllerBase
{
    private readonly AdminChargeTransactionService _chargeTransactionService;
    private readonly ILogger<AdminChargeTransactionController> _logger;

    public AdminChargeTransactionController(
        AdminChargeTransactionService chargeTransactionService,
        ILogger<AdminChargeTransactionController> logger)
    {
        _chargeTransactionService = chargeTransactionService;
        _logger = logger;
    }

    // GET: api/ChargeTransaction
    [HttpGet]
    public async Task<IActionResult> GetAllChargeTransactions()
    {
        _logger.LogInformation("Fetching all charge transactions...");
        var response = await _chargeTransactionService.GetAllChargeTransactionsAsync();
        if (response.IsSuccessful)
        {
            _logger.LogInformation("Successfully retrieved all charge transactions.");
            return Ok(response);
        }

        _logger.LogError("Error retrieving charge transactions: {Message}", response.ErrorResponse?.ResponseMessage);
        return StatusCode(500, response.ErrorResponse);
    }

    // GET: api/ChargeTransaction/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetChargeTransactionById(string id)
    {
        _logger.LogInformation("Fetching charge transaction with ID: {Id}", id);
        var response = await _chargeTransactionService.GetChargeTransactionByIdAsync(id);
        if (response.IsSuccessful)
        {
            _logger.LogInformation("Successfully retrieved charge transaction with ID: {Id}", id);
            return Ok(response);
        }

        if (response.ErrorResponse.ResponseCode == "404")
        {
            _logger.LogWarning("Charge transaction not found with ID: {Id}", id);
            return NotFound(response.ErrorResponse);
        }

        _logger.LogError("Error retrieving charge transaction with ID: {Id}. Message: {Message}", id, response.ErrorResponse?.ResponseMessage);
        return StatusCode(500, response.ErrorResponse);
    }
}
