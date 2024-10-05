using Lyvads.Application.Dtos;
using Lyvads.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminDisputeController : ControllerBase
{
    private readonly IDisputeService _disputeService;
    private readonly ILogger<AdminDisputeController> _logger;

    public AdminDisputeController(IDisputeService disputeService,
        ILogger<AdminDisputeController> logger)
    {
        _disputeService = disputeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDisputes()
    {
        var result = await _disputeService.GetAllDisputesAsync();
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDisputeDetails(string id)
    {
        var result = await _disputeService.GetDisputeDetailsAsync(id);
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }
}
