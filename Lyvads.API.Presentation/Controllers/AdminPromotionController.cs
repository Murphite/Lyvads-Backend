using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AdminPromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;
    private readonly ILogger<AdminPromotionsController> _logger;

    public AdminPromotionsController(IPromotionService promotionService, 
        ILogger<AdminPromotionsController> logger)
    {
        _promotionService = promotionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddPromotion([FromForm] CreatePromotionDto createPromotionDto)
    {
        var result = await _promotionService.AddPromotion(createPromotionDto);
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    
    [HttpPut("{promotionId}")]
    public async Task<IActionResult> UpdatePromotion(string promotionId, [FromForm] UpdatePromotionDto updatePromotionDto)
    {
        var result = await _promotionService.UpdatePromotion(promotionId, updatePromotionDto);
        if(!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    
    [HttpDelete("{promotionId}")]
    public async Task<IActionResult> DeletePromotion(string promotionId)
    {
        var result = await _promotionService.DeletePromotion(promotionId);
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    
    [HttpPut("{promotionId}/toggle-visibility")]
    public async Task<IActionResult> TogglePromotionVisibility(string promotionId, [FromQuery] bool hide)
    {
        var result = await _promotionService.TogglePromotionVisibility(promotionId, hide);
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }
}
