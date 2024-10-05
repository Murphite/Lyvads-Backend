using Lyvads.Application.Dtos;
using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminChargeController : ControllerBase
{
    private readonly IAdminChargeTransactionService _chargeTransactionService;
    private readonly ILogger<AdminChargeController> _logger;
    private readonly IAdminActivityLogService _activityLogService;

    public AdminChargeController(AdminChargeTransactionService chargeTransactionService,
        ILogger<AdminChargeController> logger,
        IAdminActivityLogService activityLogService)
    {
        _chargeTransactionService = chargeTransactionService;
        _logger = logger;
        _activityLogService = activityLogService;
    }

    // GET: api/Charge
    [HttpGet]
    public async Task<IActionResult> GetAllCharges()
    {
        _logger.LogInformation("Fetching all charges...");
        var result = await _chargeTransactionService.GetAllChargesAsync();

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    // GET: api/Charge/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetChargeById(string id)
    {
        _logger.LogInformation("Fetching charge with ID: {Id}", id);
        var result = await _chargeTransactionService.GetChargeByIdAsync(id);
        
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    // POST: api/ChargeTransaction
    [HttpPost]
    public async Task<IActionResult> AddNewCharge([FromBody] CreateChargeDto chargeDto)
    {
        _logger.LogInformation("Adding new charge...");
       
        var result = await _chargeTransactionService.AddNewChargeAsync(chargeDto);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    // PUT: api/ChargeTransaction/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> EditCharge(string id, [FromBody] EditChargeDto chargeDto)
    {
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
        _logger.LogInformation("Deleting charge with ID: {Id}", id);
        var result = await _chargeTransactionService.DeleteChargeAsync(id);
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }
}
