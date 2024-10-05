using Lyvads.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Lyvads.Application.Implementations;
using Lyvads.Application.Dtos.SuperAdminDtos;

namespace Lyvads.API.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminPermissionsController : Controller
{
    private readonly IAdminPermissionsService _adminPermissionsService;
    private readonly IAdminActivityLogService _activityLogService;
    private readonly ILogger<AdminPermissionsController> _logger;

    public AdminPermissionsController(IAdminActivityLogService activityLogService,
        ILogger<AdminPermissionsController> logger,
        IAdminPermissionsService adminPermissionsService)
    {
        _activityLogService = activityLogService;
        _logger = logger;
        _adminPermissionsService = adminPermissionsService;
    }

    [HttpGet("all-admin-users")]
    public async Task<IActionResult> GetAllAdminUsersAsync()
    {
        var result = await _adminPermissionsService.GetAllAdminUsersAsync();

        if (!result.IsSuccessful)
        {
            return NotFound(result.ResponseMessage);
        }

        return Ok(result.Data);
    }

    [HttpPost("grant-permissions/{adminUserId}")]
    public async Task<IActionResult> GrantPermissionsToAdminAsync(string adminUserId, [FromBody] AdminPermissionsDto permissionsDto, [FromHeader] string requestingAdminId)
    {
        var result = await _adminPermissionsService.GrantPermissionsToAdminAsync(adminUserId, permissionsDto, requestingAdminId);

        if (!result.IsSuccessful)
        {
            return StatusCode(int.Parse(result.ResponseCode), result.ResponseMessage);
        }

        return Ok(result.Data);
    }


    [HttpPost("create-custom-role")]
    public async Task<IActionResult> CreateCustomRoleAsync([FromBody] CreateCustomRoleRequestDto requestDto)
    {
        var result = await _adminPermissionsService.CreateCustomRoleAsync(requestDto.RoleName, requestDto.Permissions);

        if (!result.IsSuccessful)
        {
            return BadRequest(result.ResponseMessage);
        }

        return Ok(result.ResponseMessage);
    }


    [HttpPut("edit-admin-user")]
    public async Task<IActionResult> EditAdminUserAsync([FromBody] EditAdminUserDto editAdminUserDto)
    {
        var result = await _adminPermissionsService.EditAdminUserAsync(editAdminUserDto);

        if (result == null)
        {
            return NotFound("Admin user not found or could not be edited.");
        }

        return Ok(result);
    }


    [HttpPost("add-admin-user")]
    public async Task<IActionResult> AddAdminUserAsync([FromBody] AddAdminUserDto addAdminUserDto)
    {
        var result = await _adminPermissionsService.AddAdminUserAsync(addAdminUserDto);

        if (result == null)
        {
            return BadRequest("Failed to create admin user.");
        }

        return Ok(result);
    }

}
