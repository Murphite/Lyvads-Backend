using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminActivityLogController : ControllerBase
{
    private readonly IAdminActivityLogService _activityLogService;
    private readonly ILogger<AdminActivityLogController> _logger;

    public AdminActivityLogController(IAdminActivityLogService activityLogService,
        ILogger<AdminActivityLogController> logger)
    {
        _activityLogService = activityLogService;
        _logger = logger;
    }

    //[HttpPost("LogActivity")]
    //public async Task<IActionResult> LogActivityAsync(string userId, string userName, string role, string description, string category)
    //{
    //    _logger.LogInformation("Logging activity for user: {UserName}", userName);
    //    var result = await _activityLogService.LogActivityAsync(userId, userName, role, description, category);

    //    if (result.IsFailure)
    //        return BadRequest(result.ErrorResponse);

    //    return Ok(result);
    //}

    [HttpGet("all")]
    public async Task<ActionResult<List<ActivityLogDto>>> GetAllActivityLogs()
    {
        _logger.LogInformation("Fetching all activities");
        var result = await _activityLogService.GetAllActivityLogsAsync();

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }


    [HttpGet("GetActivities/{userId}")]
    public async Task<IActionResult> GetActivitiesAsync(string userId)
    {
        _logger.LogInformation("Fetching activities for user ID: {UserId}", userId);
        var result = await _activityLogService.GetActivitiesByUserIdAsync(userId);
        
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }
}
