using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Responses;
using Lyvads.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Forwarding;
using System.Security.Claims;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CollaborationController : ControllerBase
{
    private readonly ICollaborationService _collaborationService;
    private readonly ILogger<CollaborationController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;


    public CollaborationController(ICollaborationService collaborationService, 
        ILogger<CollaborationController> logger,
        UserManager<ApplicationUser> userManager
        )
    {
        _collaborationService = collaborationService;
        _logger = logger;
        _userManager = userManager;
    }


    [HttpGet("creator")]
    public async Task<ActionResult<ServerResponse<List<GetUserRequestDto>>>> GetAllRequestsForCreator([FromQuery] RequestStatus status)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return new ServerResponse<List<GetUserRequestDto>>
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User not logged in.",
                Data = null!
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles == null || roles.Contains("RegularUser"))
        {
            return new ServerResponse<List<GetUserRequestDto>>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "Only Creators are authorized.",
                Data = null!
            };
        }

        var response = await _collaborationService.GetAllRequestsForCreatorAsync(user.Id, status);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);

        return Ok(response);
    }

    [HttpGet("regularUser")]
    public async Task<ActionResult<ServerResponse<List<GetRequestDto>>>> GetAllRequestsByUser([FromQuery] RequestStatus status)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return new ServerResponse<List<GetRequestDto>>
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User not logged in.",
                Data = null!
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles == null || roles.Contains("Creator"))
        {
            return new ServerResponse<List<GetRequestDto>>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "Only Regular Users are authorized.",
                Data = null!
            };
        }

        var response = await _collaborationService.GetAllRequestsByUserAsync(user.Id, status);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);

        return Ok(response);
    }


    [HttpGet("{requestId}")]
    public async Task<ActionResult<ServerResponse<RequestDetailsDto>>> GetRequestDetails(string requestId)
    {
        var response = await _collaborationService.GetRequestDetailsAsync(requestId);
        if (!response.IsSuccessful)
        {
            return BadRequest(response.ErrorResponse);
        }
        return Ok(response);
    }


    [HttpPost("dispute/open/{requestId}")]
    public async Task<ActionResult> OpenDispute(string requestId, DisputeReasons disputeReason, 
        [FromBody] OpenDisputeDto disputeDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("User not authenticated.");
        }


        var response = await _collaborationService.OpenDisputeAsync(user.Id, requestId, disputeReason, disputeDto);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);

        return Ok(response);
    }


    [HttpPost("send-video/{requestId}")]
    public async Task<IActionResult> SendVideoToUser(string requestId, [FromForm] UploadVideo videoDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("User not authenticated.");
        }

        // Check if the video is provided
        if (videoDto.Video == null)
        {
            return BadRequest(new { message = "No video file provided." });
        }

        // Validate the file (video) if provided
        if (!IsValidVideoFile(videoDto.Video)) // Use the same validation logic as for images
        {
            return BadRequest(new { message = "Invalid File Extension" });
        }

        // Convert IFormFile to byte array
        byte[] videoBytes;
        using (var stream = new MemoryStream())
        {
            await videoDto.Video.CopyToAsync(stream);
            videoBytes = stream.ToArray();
        }

        // Call the service with the video file and other post details
        var result = await _collaborationService.SendVideoToUserAsync(requestId, videoDto.Video); // Pass the byte array

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    private bool IsValidVideoFile(IFormFile file)
    {
        var validExtensions = new[] { ".mp4", ".avi", ".mov" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        return validExtensions.Contains(extension);
    }

   
    [HttpGet("fetch-disputes")]
    public async Task<IActionResult> FetchDisputesByCreator()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not authenticated.");

        var response = await _collaborationService.FetchDisputesByCreatorAsync(user.Id);

        if (!response.IsSuccessful)
        {
            return StatusCode(int.Parse(response.ResponseCode), response);
        }

        return Ok(response);
    }


    [HttpGet("dispute-details/{disputeId}")]
    public async Task<IActionResult> GetDisputeDetails(string disputeId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not authenticated.");

        var response = await _collaborationService.GetDisputeDetailsByIdAsync(disputeId, user.Id);

        if (!response.IsSuccessful)
        {
            return StatusCode(int.Parse(response.ResponseCode), response);
        }

        return Ok(response);
    }

}
