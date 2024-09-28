using Lyvads.API.Controllers;
using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }


    [HttpPost("register")]
    public async Task<ActionResult<ServerResponse<AddUserResponseDto>>> RegisterAdmin([FromBody] RegisterAdminDto registerAdminDto)
    {
        if (registerAdminDto == null)
        {
            _logger.LogWarning("RegisterAdmin called with null RegisterAdminDto");
            return BadRequest(new ServerResponse<AddUserResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Input.Error",
                    ResponseMessage = "Invalid input data"
                }
            });
        }

        var response = await _adminService.RegisterAdmin(registerAdminDto);

        if (!response.IsSuccessful)
        {
            _logger.LogWarning("Admin registration failed: {ResponseCode}, {ResponseMessage}", response.ErrorResponse.ResponseCode, response.ErrorResponse.ResponseMessage);
            return StatusCode(int.Parse(response.ErrorResponse.ResponseCode), response);
        }

        return Ok(response);
    }

    [HttpGet("dashboard-summary")]
    public async Task<ActionResult<ServerResponse<DashboardSummaryDto>>> GetDashboardSummary()
    {
        var response = await _adminService.GetDashboardSummary();

        if (!response.IsSuccessful)
        {
            _logger.LogWarning("Fetching dashboard summary failed: {ResponseCode}, {ResponseMessage}", response.ErrorResponse.ResponseCode, response.ErrorResponse.ResponseMessage);
            return StatusCode(int.Parse(response.ErrorResponse.ResponseCode), response);
        }

        return Ok(response);
    }

    [HttpGet("revenue-report")]
    public async Task<ActionResult<ServerResponse<RevenueReportDto>>> GetRevenueReport()
    {
        var response = await _adminService.GetRevenueReport();

        if (!response.IsSuccessful)
        {
            _logger.LogWarning("Fetching revenue report failed: {ResponseCode}, {ResponseMessage}", response.ErrorResponse.ResponseCode, response.ErrorResponse.ResponseMessage);
            return StatusCode(int.Parse(response.ErrorResponse.ResponseCode), response);
        }

        return Ok(response);
    }

    [HttpGet("top-requests")]
    public async Task<ActionResult<ServerResponse<List<TopRequestDto>>>> GetTopRequests()
    {
        var response = await _adminService.GetTopRequests();

        if (!response.IsSuccessful)
        {
            _logger.LogWarning("Fetching top requests failed: {ResponseCode}, {ResponseMessage}", response.ErrorResponse.ResponseCode, response.ErrorResponse.ResponseMessage);
            return StatusCode(int.Parse(response.ErrorResponse.ResponseCode), response);
        }

        return Ok(response);
    }

    [HttpGet("top-creators")]
    public async Task<ActionResult<ServerResponse<List<TopCreatorDto>>>> GetTopCreators()
    {
        var response = await _adminService.GetTopCreators();

        if (!response.IsSuccessful)
        {
            _logger.LogWarning("Fetching top creators failed: {ResponseCode}, {ResponseMessage}", response.ErrorResponse.ResponseCode, response.ErrorResponse.ResponseMessage);
            return StatusCode(int.Parse(response.ErrorResponse.ResponseCode), response);
        }

        return Ok(response);
    }

    [HttpGet("collaboration-statuses")]
    public async Task<ActionResult<ServerResponse<CollaborationStatusReportDto>>> GetCollaborationStatusesReport()
    {
        var response = await _adminService.GetCollaborationStatusesReport();

        if (!response.IsSuccessful)
        {
            _logger.LogWarning("Fetching collaboration statuses report failed: {ResponseCode}, {ResponseMessage}", response.ErrorResponse.ResponseCode, response.ErrorResponse.ResponseMessage);
            return StatusCode(int.Parse(response.ErrorResponse.ResponseCode), response);
        }

        return Ok(response);
    }

}
