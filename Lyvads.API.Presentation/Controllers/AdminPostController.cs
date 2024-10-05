using Lyvads.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminPostController : ControllerBase
{
    private readonly IAdminPostService _adminPostService;
    private readonly ILogger<AdminPostController> _logger;

    public AdminPostController(IAdminPostService adminPostService, ILogger<AdminPostController> logger)
    {
        _adminPostService = adminPostService;
        _logger = logger;
    }

    
    [HttpGet("GetAllPosts")]
    public async Task<IActionResult> GetAllPostsAsync()
    {
        _logger.LogInformation("Request received for fetching all posts.");
        var response = await _adminPostService.GetAllPostsAsync();
        if (!response.IsSuccessful)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    
    [HttpGet("GetPostDetails/{postId}")]
    public async Task<IActionResult> GetPostDetailsAsync(string postId)
    {
        _logger.LogInformation("Request received for fetching details for post ID: {PostId}", postId);
        var response = await _adminPostService.GetPostDetailsAsync(postId);
        if (!response.IsSuccessful)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    
    [HttpPost("FlagPost/{postId}")]
    public async Task<IActionResult> FlagPostAsync(int postId)
    {
        _logger.LogInformation("Request received to flag post with ID: {PostId}", postId);
        var response = await _adminPostService.FlagPostAsync(postId);
        if (!response.IsSuccessful)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    
    [HttpDelete("DeletePost/{postId}")]
    public async Task<IActionResult> DeletePostAsync(int postId)
    {
        _logger.LogInformation("Request received to delete post with ID: {PostId}", postId);
        var response = await _adminPostService.DeletePostAsync(postId);
        if (!response.IsSuccessful)
        {
            return NotFound(response);
        }
        return Ok(response);
    }
}
