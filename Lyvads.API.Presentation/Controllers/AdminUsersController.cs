using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace Lyvads.API.Presentation.Controllers;

[ApiController]
//[Authorize]
[Route("api/[controller]")]
public class AdminUsersController : Controller
{
    private readonly ISuperAdminService _superAdminService;
    private readonly ILogger<AdminUsersController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUsersController(ISuperAdminService superAdminService,
        ILogger<AdminUsersController> logger,
        UserManager<ApplicationUser> userManager)
    {
        _superAdminService = superAdminService;
        _logger = logger;
        _userManager = userManager;
    }


    [HttpGet("get-users")]
    public async Task<IActionResult> GetUsers([FromQuery] string role = null!, [FromQuery] bool sortByDate = true)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var response = await _superAdminService.GetUsers(role, sortByDate);
        if (!response.IsSuccessful)
        {
            _logger.LogError("Failed to fetch users: {Error}", response.ErrorResponse.ResponseDescription);
            return StatusCode(500, response.ErrorResponse);
        }
        return Ok(response);
    }


    [HttpPost("add-user")]
    public async Task<IActionResult> AddUser([FromBody] AdminRegisterUserDto registerUserDto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var response = await _superAdminService.AddUser(registerUserDto);
        if (!response.IsSuccessful)
        {
            _logger.LogError("User registration failed: {Error}", response.ErrorResponse.ResponseDescription);
            return StatusCode(400, response.ErrorResponse);
        }
        return Ok(response);
    }


    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var response = await _superAdminService.DeleteUser(userId);
        if (!response.IsSuccessful)
        {
            _logger.LogError("User deletion failed: {Error}", response.ErrorResponse.ResponseDescription);
            return StatusCode(500, response.ErrorResponse);
        }
        return Ok(response);
    }


    [HttpPost("{userId}/toggle-activate/disable")]
    public async Task<IActionResult> ToggleUserStatus(string userId)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var response = await _superAdminService.ToggleUserStatusAsync(userId);
        if (!response.IsSuccessful)
        {
            _logger.LogError("User disable failed: {Error}", response.ErrorResponse.ResponseDescription);
            return StatusCode(500, response.ErrorResponse);
        }
        return Ok(response);
    }



    //[HttpPost("{userId}/disable")]
    //public async Task<IActionResult> DisableUser(string userId)
    //{
    //    var response = await _superAdminService.DisableUser(userId);
    //    if (!response.IsSuccessful)
    //    {
    //        _logger.LogError("User disable failed: {Error}", response.ErrorResponse.ResponseDescription);
    //        return StatusCode(500, response.ErrorResponse);
    //    }
    //    return Ok(response);
    //}


    //[HttpPost("{userId}/activate")]
    //public async Task<IActionResult> ActivateUser(string userId)
    //{
    //    var response = await _superAdminService.ActivateUserAsync(userId);
    //    if (!response.IsSuccessful)
    //    {
    //        _logger.LogError("User activation failed: {Error}", response.ErrorResponse.ResponseDescription);
    //        return StatusCode(500, response.ErrorResponse);
    //    }
    //    return Ok(response);
    //}
}
