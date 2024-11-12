using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AdminPromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;
    private readonly ILogger<AdminPromotionsController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    public AdminPromotionsController(IPromotionService promotionService, 
        ILogger<AdminPromotionsController> logger,
        UserManager<ApplicationUser> userManager)
    {
        _promotionService = promotionService;
        _logger = logger;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> AddPromotion([FromForm] CreatePromotionDto createPromotionDto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _promotionService.AddPromotion(createPromotionDto);
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    
    [HttpPost("{promotionId}")]
    public async Task<IActionResult> UpdatePromotion(string promotionId, [FromForm] UpdatePromotionDto updatePromotionDto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _promotionService.UpdatePromotion(promotionId, updatePromotionDto);
        if(!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    
    [HttpDelete("{promotionId}")]
    public async Task<IActionResult> DeletePromotion(string promotionId)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _promotionService.DeletePromotion(promotionId);
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }


    [HttpPost("{promotionId}/toggle-visibility")]
    public async Task<IActionResult> TogglePromotionVisibility(string promotionId)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _promotionService.TogglePromotionVisibility(promotionId);
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPromotion()
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _promotionService.GetAllPromotions();
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }
}
