using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Implementations;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Forwarding;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CollaborationController : ControllerBase
{
    private readonly CollaborationService _collaborationService;
    private readonly ILogger<CollaborationController> _logger;

    public CollaborationController(CollaborationService collaborationService, ILogger<CollaborationController> logger)
    {
        _collaborationService = collaborationService;
        _logger = logger;
    }


    [HttpGet("creator/{creatorId}")]
    public async Task<ActionResult<ServerResponse<List<GetUserRequestDto>>>> GetAllRequestsForCreator(string creatorId, [FromQuery] string status)
    {
        var response = await _collaborationService.GetAllRequestsForCreatorAsync(creatorId, status);
        if (!response.IsSuccessful)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ServerResponse<List<GetRequestDto>>>> GetAllRequestsByUser(string userId, [FromQuery] string status)
    {
        var response = await _collaborationService.GetAllRequestsByUserAsync(userId, status);
        if (!response.IsSuccessful)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    [HttpGet("{requestId}")]
    public async Task<ActionResult<ServerResponse<RequestDetailsDto>>> GetRequestDetails(string requestId)
    {
        var response = await _collaborationService.GetRequestDetailsAsync(requestId);
        if (!response.IsSuccessful)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    [HttpPost("dispute/open/{requestId}")]
    public async Task<ActionResult<ServerResponse<DisputeResponseDto>>> OpenDispute(string requestId, [FromBody] OpenDisputeDto disputeDto)
    {
        var response = await _collaborationService.OpenDisputeAsync(requestId, disputeDto);
        if (!response.IsSuccessful)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [HttpPost("send-video/{requestId}")]
    public async Task<IActionResult> SendVideoToUser(string requestId, IFormFile video)
    {
        var response = await _collaborationService.SendVideoToUserAsync(requestId, video);

        if (!response.IsSuccessful)
        {
            return StatusCode(int.Parse(response.ResponseCode), response);
        }

        return Ok(response);
    }

    [HttpGet("fetch-disputes/{userId}")]
    public async Task<IActionResult> FetchDisputesByCreator(string userId)
    {
        var response = await _collaborationService.FetchDisputesByCreatorAsync(userId);

        if (!response.IsSuccessful)
        {
            return StatusCode(int.Parse(response.ResponseCode), response);
        }

        return Ok(response);
    }


    [HttpGet("dispute-details/{disputeId}")]
    public async Task<IActionResult> GetDisputeDetails(string disputeId, [FromQuery] string userId)
    {
        var response = await _collaborationService.GetDisputeDetailsByIdAsync(disputeId, userId);

        if (!response.IsSuccessful)
        {
            return StatusCode(int.Parse(response.ResponseCode), response);
        }

        return Ok(response);
    }

}
