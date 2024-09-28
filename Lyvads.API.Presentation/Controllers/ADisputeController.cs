using Lyvads.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DisputeController : ControllerBase
{
    private readonly IDisputeService _disputeService;

    public DisputeController(IDisputeService disputeService)
    {
        _disputeService = disputeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDisputes()
    {
        var response = await _disputeService.GetAllDisputesAsync();
        if (response.IsSuccessful)
        {
            return Ok(response); 
        }
        return StatusCode(500, response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDisputeDetails(string id)
    {
        var response = await _disputeService.GetDisputeDetailsAsync(id);
        if (response.IsSuccessful)
            return Ok(response);

        if (response.ErrorResponse.ResponseCode == "404")
            return NotFound(response);

        return StatusCode(500, response); 
    }
}
