using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class APromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public APromotionsController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpPost]
    public async Task<IActionResult> AddPromotion([FromForm] CreatePromotionDto createPromotionDto)
    {
        var result = await _promotionService.AddPromotion(createPromotionDto);
        if (result.IsFailure)
            return BadRequest(result.ErrorResponse);

        return Ok(result);

    }

    [HttpPut("{promotionId}")]
    public async Task<IActionResult> UpdatePromotion(string promotionId, [FromForm] UpdatePromotionDto updatePromotionDto)
    {
        var result = await _promotionService.UpdatePromotion(promotionId, updatePromotionDto);
        if(result.IsFailure)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    [HttpDelete("{promotionId}")]
    public async Task<IActionResult> DeletePromotion(string promotionId)
    {
        var result = await _promotionService.DeletePromotion(promotionId);
        if (result.IsFailure)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    [HttpPut("{promotionId}/toggle-visibility")]
    public async Task<IActionResult> TogglePromotionVisibility(string promotionId, [FromQuery] bool hide)
    {
        var result = await _promotionService.TogglePromotionVisibility(promotionId, hide);
        if (result.IsFailure)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }
}
