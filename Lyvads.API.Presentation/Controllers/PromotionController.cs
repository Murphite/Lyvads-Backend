using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lyvads.API.Presentation.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PromotionController : ControllerBase
{
    private readonly IPromotionPlanService _promotionService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PromotionController(IPromotionPlanService promotionService, UserManager<ApplicationUser> userManager)
    {
        _promotionService = promotionService;
        _userManager = userManager;
    }

    [HttpPost("create-plan")]
    public async Task<IActionResult> CreatePromotionPlan([FromBody] CreatePromotionPlanDto planDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authenticated."
            });

        var result = await _promotionService.CreatePromotionPlanAsync(planDto);

        if (!result.IsSuccessful)
            return BadRequest(result);

        return Ok(result);
    }

   
    [HttpGet("available-plans")]
    public async Task<IActionResult> GetAvailablePromotionPlans([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authenticated."
            });

        var paginationFilter = new PaginationFilter(pageNumber, pageSize);
        var result = await _promotionService.GetAvailablePromotionPlansAsync(paginationFilter);

        if (!result.IsSuccessful)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpPost("subscribe")]
    public async Task<IActionResult> SubscribeToPromotionPlan([FromQuery] string planId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authenticated."
            });

        var result = await _promotionService.SubscribeToPromotionPlanAsync(planId, userId);

        if (!result.IsSuccessful)
            return BadRequest(result);

        return Ok(result);
    }

    
    [HttpGet("subscribed-creators")]
    public async Task<IActionResult> GetSubscribedCreators([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authenticated."
            });

        var paginationFilter = new PaginationFilter(pageNumber, pageSize);
        var result = await _promotionService.GetSubscribedCreatorsAsync(paginationFilter);

        if (!result.IsSuccessful)
            return BadRequest(result);

        return Ok(result);
    }
}
