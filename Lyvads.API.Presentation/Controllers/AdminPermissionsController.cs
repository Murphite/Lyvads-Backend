using Lyvads.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Lyvads.Application.Implementations;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Lyvads.Domain.Entities;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Domain.Responses;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AdminPermissionsController : Controller
{
    private readonly IAdminPermissionsService _adminPermissionsService;
    private readonly IAdminActivityLogService _activityLogService;
    private readonly ILogger<AdminPermissionsController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminPermissionsController(IAdminActivityLogService activityLogService,
        ILogger<AdminPermissionsController> logger,
        IAdminPermissionsService adminPermissionsService,
        UserManager<ApplicationUser> userManager)
    {
        _activityLogService = activityLogService;
        _logger = logger;
        _adminPermissionsService = adminPermissionsService;
        _userManager = userManager;
    }

    [HttpGet("all-admin-users")]
    public async Task<IActionResult> GetAllAdminUsersAsync()
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var roles = await _userManager.GetRolesAsync(user);
        if (roles == null || !roles.Any(r => string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            return Unauthorized("Only Super Admins are authorized");

        var result = await _adminPermissionsService.GetAllAdminUsersAsync();

        if (!result.IsSuccessful)
            return BadRequest(result);

        return Ok(result);
    }

   
    [HttpPost("grant-permissions")]
    public async Task<IActionResult> GrantPermissionsToAdminAsync([FromBody] AdminPermissionsDto permissionsDto, 
        [FromHeader] string targetAdminId)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var roles = await _userManager.GetRolesAsync(user);
        if (roles == null || !roles.Any(r => string.Equals(r, "SUPERADMIN", StringComparison.OrdinalIgnoreCase)))
            return Unauthorized("Only Super Admins are authorized");

        var result = await _adminPermissionsService.GrantPermissionsToAdminAsync(user.Id, permissionsDto, targetAdminId);

        if (!result.IsSuccessful)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpPost("create-custom-role")]
    public async Task<IActionResult> CreateCustomRoleAsync([FromBody] CreateCustomRoleRequestDto requestDto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        if (string.IsNullOrWhiteSpace(requestDto?.RoleName))
            return BadRequest("Role name is required.");

        var roles = await _userManager.GetRolesAsync(user);
        if (roles == null || !roles.Any(r => string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            return Unauthorized("Only Super Admins are authorized");

        if (requestDto?.Permissions == null)
            return BadRequest("Permissions are required.");

        var result = await _adminPermissionsService.CreateCustomRoleAsync(requestDto.RoleName, requestDto.Permissions);

        if (!result.IsSuccessful)
            return BadRequest(result);

        return Ok(result);
    }

    
    [HttpGet("get-all-roles-with-permissions")]
    public async Task<IActionResult> GetAllRolesWithPermissionsAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var roles = await _userManager.GetRolesAsync(user);
        if (roles == null || !roles.Any(r => string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            return Unauthorized("Only Super Admins are authorized");

        var result = await _adminPermissionsService.GetAllRolesWithPermissionsAsync();

        if (!result.IsSuccessful)
            return NotFound(result);

        return Ok(result);
    }


    [HttpPost("add-admin-user")]
    public async Task<IActionResult> AddAdminUserAsync([FromBody] AddAdminUserDto addAdminUserDto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var roles = await _userManager.GetRolesAsync(user);
        if (roles == null || !roles.Any(r => string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            return Unauthorized("Only Super Admins are authorized");

        var result = await _adminPermissionsService.AddAdminUserAsync(addAdminUserDto);
        if (!result.IsSuccessful)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpPost("edit-admin-user")]
    public async Task<IActionResult> EditAdminUserAsync([FromQuery] string adminUserId, [FromBody] EditAdminUserDto editAdminUserDto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var roles = await _userManager.GetRolesAsync(user);
        if (roles == null || !roles.Any(r => string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            return Unauthorized("Only Super Admins are authorized");

        var result = await _adminPermissionsService.EditAdminUserAsync(adminUserId, editAdminUserDto);

        if (!result.IsSuccessful)
            return BadRequest(result);

        return Ok(result);
    }

   
    [HttpDelete("delete-admin-user")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var roles = await _userManager.GetRolesAsync(user);
        if (roles == null || !roles.Any(r => string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            return Unauthorized("Only Super Admins are authorized");

        var response = await _adminPermissionsService.DeleteAdminUserAsync(userId);
        if (!response.IsSuccessful)
        {
            _logger.LogError("User deletion failed: {Error}", response.ErrorResponse.ResponseDescription);
            return StatusCode(500, response.ErrorResponse);
        }
        return Ok(response);
    }
}
