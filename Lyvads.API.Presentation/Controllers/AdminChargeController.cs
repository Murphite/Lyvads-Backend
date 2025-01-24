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
public class AdminChargeController : ControllerBase
{
    private readonly IAdminChargeTransactionService _chargeTransactionService;
    private readonly ILogger<AdminChargeController> _logger;
    private readonly IAdminActivityLogService _activityLogService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminChargeController(
        IAdminChargeTransactionService chargeTransactionService,
        ILogger<AdminChargeController> logger,
        IAdminActivityLogService activityLogService,
        UserManager<ApplicationUser> userManager)
    {
        _chargeTransactionService = chargeTransactionService;
        _logger = logger;
        _activityLogService = activityLogService;
        _userManager = userManager;
    }

    // GET: api/Charge
    [HttpGet("GetAllCharges")]
    public async Task<IActionResult> GetAllCharges()
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        _logger.LogInformation("Fetching all charges...");
        var result = await _chargeTransactionService.GetAllChargesAsync();

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    // GET: api/Charge/{id}
    [HttpGet("ChargeById")]
    public async Task<IActionResult> GetChargeById(string id)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        _logger.LogInformation("Fetching charge with ID: {Id}", id);
        var result = await _chargeTransactionService.GetChargeByIdAsync(id);
        
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    // POST: api/ChargeTransaction
    [HttpPost("AddNewCharge")]
    public async Task<IActionResult> AddNewCharge([FromBody] CreateChargeDto chargeDto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        _logger.LogInformation("Adding new charge...");
       
        var result = await _chargeTransactionService.AddNewChargeAsync(chargeDto);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    // PUT: api/ChargeTransaction/{id}
    [HttpPost("EditCharge")]
    public async Task<IActionResult> EditCharge(string id, [FromBody] EditChargeDto chargeDto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        _logger.LogInformation("Editing charge with ID: {Id}", id);
       
        var result = await _chargeTransactionService.EditChargeAsync(id, chargeDto);
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    // DELETE: api/ChargeTransaction/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCharge(string id)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        _logger.LogInformation("Deleting charge with ID: {Id}", id);
        var result = await _chargeTransactionService.DeleteChargeAsync(id);
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }
}
