using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ASuperAdminController : Controller
{
    private readonly ISuperAdminService _superAdminService;
    private readonly ILogger<ASuperAdminController> _logger;

    public ASuperAdminController(ISuperAdminService superAdminService,
        ILogger<ASuperAdminController> logger)
    {
        _superAdminService = superAdminService;
        _logger = logger;
    }


    [HttpGet("get-users")]
    public async Task<IActionResult> GetUsers([FromQuery] string role = null, [FromQuery] bool sortByDate = true)
    {
        var response = await _superAdminService.GetUsers(role, sortByDate);
        if (!response.IsSuccessful)
        {
            _logger.LogError("Failed to fetch users: {Error}", response.ErrorResponse.ResponseDescription);
            return StatusCode(500, response.ErrorResponse);
        }
        return Ok(response.Data);
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto registerUserDto)
    {
        var response = await _superAdminService.RegisterUser(registerUserDto);
        if (!response.IsSuccessful)
        {
            _logger.LogError("User registration failed: {Error}", response.ErrorResponse.ResponseDescription);
            return StatusCode(400, response.ErrorResponse);
        }
        return Ok(response.Data);
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var response = await _superAdminService.DeleteUser(userId);
        if (!response.IsSuccessful)
        {
            _logger.LogError("User deletion failed: {Error}", response.ErrorResponse.ResponseDescription);
            return StatusCode(500, response.ErrorResponse);
        }
        return Ok(new { message = "User deleted successfully." });
    }

    [HttpPost("{userId}/disable")]
    public async Task<IActionResult> DisableUser(string userId)
    {
        var response = await _superAdminService.DisableUser(userId);
        if (!response.IsSuccessful)
        {
            _logger.LogError("User disable failed: {Error}", response.ErrorResponse.ResponseDescription);
            return StatusCode(500, response.ErrorResponse);
        }
        return Ok(new { message = "User disabled successfully." });
    }

    [HttpPost("{userId}/activate")]
    public async Task<IActionResult> ActivateUser(string userId)
    {
        var response = await _superAdminService.ActivateUser(userId);
        if (!response.IsSuccessful)
        {
            _logger.LogError("User activation failed: {Error}", response.ErrorResponse.ResponseDescription);
            return StatusCode(500, response.ErrorResponse);
        }
        return Ok(new { message = "User activated successfully." });
    }
}
