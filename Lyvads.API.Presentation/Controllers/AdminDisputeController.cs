using Lyvads.Application.Dtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AdminDisputeController : ControllerBase
{
    private readonly IDisputeService _disputeService;
    private readonly ILogger<AdminDisputeController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminDisputeController(IDisputeService disputeService,
        ILogger<AdminDisputeController> logger,
        UserManager<ApplicationUser> userManager)
    {
        _disputeService = disputeService;
        _logger = logger;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDisputes()
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");


        var result = await _disputeService.GetAllDisputesAsync();
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDisputeDetails(string id)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _disputeService.GetDisputeDetailsAsync(id);
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }
}
