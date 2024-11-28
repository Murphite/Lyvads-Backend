using Lyvads.Application.Dtos;
using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AdminChargeTransactionController : ControllerBase
{
    private readonly IAdminChargeTransactionService _chargeTransactionService;
    private readonly ILogger<AdminChargeTransactionController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminChargeTransactionController(
        IAdminChargeTransactionService chargeTransactionService,
        ILogger<AdminChargeTransactionController> logger,
        UserManager<ApplicationUser> userManager
)
    {
        _chargeTransactionService = chargeTransactionService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: api/ChargeTransaction
    [HttpGet("GetAllChargeTransactions")]
    public async Task<IActionResult> GetAllChargeTransactions()
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

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
    [HttpGet("{GetChargeTransactionById}")]
    public async Task<IActionResult> GetChargeTransactionById(string id)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

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
