using Lyvads.API.Controllers;
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AdminDashboardController : Controller
{
    private readonly IAdminUserService _adminService;
    private readonly ILogger<AdminDashboardController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminDashboardController(IAdminUserService adminService,
        ILogger<AdminDashboardController> logger,
        UserManager<ApplicationUser> userManager)
    {
        _adminService = adminService;
        _logger = logger;
        _userManager = userManager;
    }


    [HttpGet("dashboard-summary")]
    public async Task<ActionResult<ServerResponse<DashboardSummaryDto>>> GetDashboardSummary()
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _adminService.GetDashboardSummary();

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    
    [HttpGet("revenue-report")]
    public async Task<ActionResult<ServerResponse<RevenueReportDto>>> GetRevenueReport()
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _adminService.GetRevenueReport();

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    [HttpGet("time-period-report")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] string timePeriod)
    {
        if (string.IsNullOrWhiteSpace(timePeriod))
        {
            return BadRequest(new
            {
                IsSuccessful = false,
                ResponseMessage = "Time period is required. Please specify 'yearly', 'monthly', 'weekly', or 'daily'."
            });
        }

        var result = await _adminService.GetRevenueReport(timePeriod);

        if (!result.IsSuccessful)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    
    [HttpGet("monthly-revenue-report")]
    public async Task<ActionResult<ServerResponse<MonthlyRevenueReportDto>>> GetMonthlyRevenueReport(int year)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _adminService.GetMonthlyRevenueReportAsync(year);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }


    [HttpGet("top-requests")]
    public async Task<ActionResult<ServerResponse<List<TopRequestDto>>>> GetTopRequests()
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _adminService.GetTopRequests();

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    
    [HttpGet("top-creators")]
    public async Task<ActionResult<ServerResponse<List<TopCreatorDto>>>> GetTopCreators()
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _adminService.GetTopCreators();

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    
    [HttpGet("collaboration-statuses")]
    public async Task<ActionResult<ServerResponse<CollaborationStatusReportDto>>> GetCollaborationStatusesReport()
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _adminService.GetCollaborationStatusesReport();

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

}
