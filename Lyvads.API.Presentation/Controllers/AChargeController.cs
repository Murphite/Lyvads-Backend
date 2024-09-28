using Lyvads.Application.Dtos;
using Lyvads.Application.Implementations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChargeController : ControllerBase
{
    private readonly ChargeTransactionService _chargeTransactionService;
    private readonly ILogger<ChargeController> _logger;

    public ChargeController(ChargeTransactionService chargeTransactionService, ILogger<ChargeController> logger)
    {
        _chargeTransactionService = chargeTransactionService;
        _logger = logger;
    }

    // GET: api/Charge
    [HttpGet]
    public async Task<IActionResult> GetAllCharges()
    {
        _logger.LogInformation("Fetching all charges...");
        var response = await _chargeTransactionService.GetAllChargesAsync();
        if (response.IsSuccessful)
        {
            _logger.LogInformation("Successfully retrieved all charges.");
            return Ok(response);
        }

        _logger.LogError("Error retrieving charges: {Message}", response.ErrorResponse?.ResponseMessage);
        return StatusCode(500, response.ErrorResponse);
    }

    // GET: api/Charge/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetChargeById(string id)
    {
        _logger.LogInformation("Fetching charge with ID: {Id}", id);
        var response = await _chargeTransactionService.GetChargeByIdAsync(id);
        if (response.IsSuccessful)
        {
            _logger.LogInformation("Successfully retrieved charge with ID: {Id}", id);
            return Ok(response);
        }

        if (response.ErrorResponse.ResponseCode == "404")
        {
            _logger.LogWarning("Charge not found with ID: {Id}", id);
            return NotFound(response.ErrorResponse);
        }

        _logger.LogError("Error retrieving charge with ID: {Id}. Message: {Message}", id, response.ErrorResponse?.ResponseMessage);
        return StatusCode(500, response.ErrorResponse);
    }

    // POST: api/ChargeTransaction
    [HttpPost]
    public async Task<IActionResult> AddNewCharge([FromBody] CreateChargeDto chargeDto)
    {
        _logger.LogInformation("Adding new charge...");
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid charge model state.");
            return BadRequest(ModelState);
        }

        var response = await _chargeTransactionService.AddNewChargeAsync(chargeDto);
        if (response.IsSuccessful)
        {
            _logger.LogInformation("Successfully added new charge.");
            return Ok(response);
        }

        _logger.LogError("Error adding new charge: {Message}", response.ErrorResponse?.ResponseMessage);
        return StatusCode(500, response.ErrorResponse);
    }

    // PUT: api/ChargeTransaction/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> EditCharge(string id, [FromBody] EditChargeDto chargeDto)
    {
        _logger.LogInformation("Editing charge with ID: {Id}", id);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid charge model state.");
            return BadRequest(ModelState);
        }

        var response = await _chargeTransactionService.EditChargeAsync(id, chargeDto);
        if (response.IsSuccessful)
        {
            _logger.LogInformation("Successfully edited charge with ID: {Id}", id);
            return Ok(response);
        }

        if (response.ErrorResponse.ResponseCode == "404")
        {
            _logger.LogWarning("Charge not found with ID: {Id}", id);
            return NotFound(response.ErrorResponse);
        }

        _logger.LogError("Error editing charge with ID: {Id}. Message: {Message}", id, response.ErrorResponse?.ResponseMessage);
        return StatusCode(500, response.ErrorResponse);
    }

    // DELETE: api/ChargeTransaction/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCharge(string id)
    {
        _logger.LogInformation("Deleting charge with ID: {Id}", id);
        var response = await _chargeTransactionService.DeleteChargeAsync(id);
        if (response.IsSuccessful)
        {
            _logger.LogInformation("Successfully deleted charge with ID: {Id}", id);
            return Ok(response);
        }

        if (response.ErrorResponse.ResponseCode == "404")
        {
            _logger.LogWarning("Charge not found with ID: {Id}", id);
            return NotFound(response.ErrorResponse);
        }

        _logger.LogError("Error deleting charge with ID: {Id}. Message: {Message}", id, response.ErrorResponse?.ResponseMessage);
        return StatusCode(500, response.ErrorResponse);
    }
}
