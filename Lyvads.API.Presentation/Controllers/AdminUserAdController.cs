using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AdminUserAdController : ControllerBase
{
    private readonly IUserAdService _userAdService;
    private readonly ILogger<AdminUserAdController> _logger;


    public AdminUserAdController(IUserAdService userAdService,
        ILogger<AdminUserAdController> logger)
    {
        _userAdService = userAdService;
        _logger = logger;
    }


    [HttpGet("all")]
    public async Task<IActionResult> GetAllUserAds()
    {
        try
        {
            var userAds = await _userAdService.GetAllUserAdsAsync();
            return Ok(userAds);
        }
        catch (Exception ex)
        {
            // Log the exception (if logging is set up) and return a generic error message
             _logger.LogError(ex, "An error occurred while fetching user ads.");
            return StatusCode(500, "Internal server error. Please try again later.");
        }
    }


    [HttpPost("approveAd/{adId}")]
    public async Task<IActionResult> ApproveAd(string adId)
    {
        try
        {
            await _userAdService.ApproveAdAsync(adId);
            return Ok(new { message = "Ad approved successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpPost("declineAd/{adId}")]
    public async Task<IActionResult> DeclineAd(string adId)
    {
        try
        {
            await _userAdService.DeclineAdAsync(adId);
            return Ok(new { message = "Ad declined successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}
