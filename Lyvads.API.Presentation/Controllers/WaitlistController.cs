using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Dtos;
using Lyvads.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaitlistController : Controller
{
    private readonly IWaitlistService _waitlistService;
    private readonly ILogger<WaitlistController> _logger;

    public WaitlistController(IWaitlistService waitlistService, ILogger<WaitlistController> logger)
    {
        _waitlistService = waitlistService;
        _logger = logger;
    }


    [HttpPost("AddUsers")]
    public async Task<IActionResult> AddToWaitlist([FromBody] WaitlistDto waitlistDto)
    {
        _logger.LogInformation("******* Inside the AddToWaitlist Controller Method ********");

        var result = await _waitlistService.AddToWaitlist(waitlistDto.Email);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success(successMessage: result.Message));
    }


    [HttpPost("Notify")]
    public async Task<IActionResult> NotifyWaitlistUsers()
    {
        _logger.LogInformation($"******* Inside the NotifyWaitlistUsers Controller Method ********");

        var result = await _waitlistService.NotifyWaitlistUsers();

        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return Ok(ResponseDto<object>.Success());
    }


}
