using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AdminCollaborationController : ControllerBase
{
    private readonly ICollaborationService _collaborationService;
    private readonly ILogger<AdminCollaborationController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminCollaborationController(ICollaborationService collaborationService, 
        ILogger<AdminCollaborationController> logger,
        UserManager<ApplicationUser> userManager)
    {
        _collaborationService = collaborationService;
        _logger = logger;
        _userManager = userManager;
    }

    // GET: api/collaboration
    [HttpGet]
    public async Task<ActionResult<ServerResponse<List<CollaborationDto>>>> GetCollaborations()
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        _logger.LogInformation("Handling request to fetch all collaborations.");
        var response = await _collaborationService.GetCollaborationsAsync();

        if (!response.IsSuccessful)
        {
            _logger.LogWarning("Failed to fetch collaborations: {ResponseMessage}", response.ResponseMessage);
            return NotFound(response);
        }

        return Ok(response);
    }

    
    // GET: api/collaboration/{id}    
    [HttpGet("{collaborationId}")]
    public async Task<ActionResult<ServerResponse<CollaborationDto>>> GetCollaborationDetails(string collaborationId)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        _logger.LogInformation("Handling request to fetch collaboration details for ID: {CollaborationId}", collaborationId);
        var response = await _collaborationService.GetCollaborationDetailsAsync(collaborationId);

        if (!response.IsSuccessful)
        {
            _logger.LogWarning("Failed to fetch collaboration details: {ResponseMessage}", response.ResponseMessage);
            return NotFound(response);
        }

        return Ok(response);
    }


    [HttpPost("ToggleCollaborationStatus/collaborationId")]
    public async Task<IActionResult> FlagToggleCollaboration(string collaborationId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        _logger.LogInformation("Request received to toggle status for post with ID: {PostId}", collaborationId);
        var response = await _collaborationService.FlagToggleCollaborationAsync(collaborationId);

        if (!response.IsSuccessful)
            return BadRequest(response);

        return Ok(response);
    }
}
