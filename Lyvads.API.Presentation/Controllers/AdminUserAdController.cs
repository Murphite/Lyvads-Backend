using Azure;
using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;


[ApiController]
//[Authorize]
[Route("api/[controller]")]
public class AdminUserAdController : ControllerBase
{
    private readonly IUserAdService _userAdService;
    private readonly ILogger<AdminUserAdController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;


    public AdminUserAdController(IUserAdService userAdService,
        ILogger<AdminUserAdController> logger,
        UserManager<ApplicationUser> userManager)
    {
        _userAdService = userAdService;
        _logger = logger;
        _userManager = userManager;
    }


    [HttpGet("all")]
    public async Task<IActionResult> GetAllUserAds()
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var response = await _userAdService.GetAllUserAdsAsync();
        if (!response.IsSuccessful)
        {
            _logger.LogError("Method Failed: {Error}", response.ErrorResponse.ResponseDescription);
            return StatusCode(500, response.ErrorResponse);
        }
        return Ok(response);

    }


    [HttpPost("toggle-activateAd-declineAd/{adId}")]
    public async Task<IActionResult> ToggleAdStatusAsync(string adId)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var response = await _userAdService.ToggleAdStatusAsync(adId);

        if (!response.IsSuccessful)
        {
            _logger.LogError("Method Failed: {Error}", response.ErrorResponse.ResponseDescription);
            return StatusCode(500, response.ErrorResponse);
        }
        return Ok(response);
    }

    //[HttpPost("approveAd/{adId}")]
    //public async Task<IActionResult> ApproveAd(string adId)
    //{
    //    var response = await _userAdService.ApproveAdAsync(adId);

    //    if (!response.IsSuccessful)
    //    {
    //        _logger.LogError("Method Failed: {Error}", response.ErrorResponse.ResponseDescription);
    //        return StatusCode(500, response.ErrorResponse);
    //    }
    //    return Ok(response);
    //}


    //[HttpPost("declineAd/{adId}")]
    //public async Task<IActionResult> DeclineAd(string adId)
    //{
    //    var response = await _userAdService.DeclineAdAsync(adId);

    //    if (!response.IsSuccessful)
    //    {
    //        _logger.LogError("Method Failed: {Error}", response.ErrorResponse.ResponseDescription);
    //        return StatusCode(500, response.ErrorResponse);
    //    }
    //    return Ok(response);
    //}
}
