using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AdminPostController : ControllerBase
{
    private readonly IAdminPostService _adminPostService;
    private readonly ILogger<AdminPostController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminPostController(IAdminPostService adminPostService, 
        ILogger<AdminPostController> logger,
        UserManager<ApplicationUser> userManager)
    {
        _adminPostService = adminPostService;
        _logger = logger;
        _userManager = userManager;
    }

    
    [HttpGet("GetAllPosts")]
    public async Task<IActionResult> GetAllPostsAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        _logger.LogInformation("Request received for fetching all posts.");

        var response = await _adminPostService.GetAllPostsAsync();
        if (!response.IsSuccessful)
            return BadRequest(response);

        return Ok(response);
    }

    
    [HttpGet("GetPostDetails/{postId}")]
    public async Task<IActionResult> GetPostDetailsAsync(string postId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        _logger.LogInformation("Request received for fetching details for post ID: {PostId}", postId);
        var response = await _adminPostService.GetPostDetailsAsync(postId);

        if (!response.IsSuccessful)
            return BadRequest(response);

        return Ok(response);
    }


    [HttpPost("TogglePostStatus/{postId}")]
    public async Task<IActionResult> TogglePostStatusAsync(string postId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        _logger.LogInformation("Request received to toggle status for post with ID: {PostId}", postId);
        var response = await _adminPostService.FlagPostAsync(postId);

        if (!response.IsSuccessful)
            return BadRequest(response);

        return Ok(response);
    }



    [HttpDelete("DeletePost/{postId}")]
    public async Task<IActionResult> DeletePostAsync(string postId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var roles = await _userManager.GetRolesAsync(user);
        if (roles == null || (!roles.Contains("SuperAdmin") && !roles.Contains("Admin")))
            return Unauthorized("Only Super Admins and Admins are authorized");

        _logger.LogInformation("Request received to delete post with ID: {PostId}", postId);

        var response = await _adminPostService.DeletePostAsync(postId);
        if (!response.IsSuccessful)
            return BadRequest(response);

        return Ok(response);
    }
}
