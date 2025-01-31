using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Repositories;
using Lyvads.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CollaborationController : ControllerBase
{
    private readonly ICollaborationService _collaborationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CollaborationController> _logger;


    public CollaborationController(ICollaborationService collaborationService, 
        ILogger<CollaborationController> logger,
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork
        )
    {
        _collaborationService = collaborationService;
        _logger = logger;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }


    [HttpGet("GetAllRequestsForCreator")]
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

    
    [HttpGet("GetAllRequestsByUser")]
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


    [HttpGet("GetRequestDetails/requestId")]
    public async Task<ActionResult<ServerResponse<RequestDetailsDto>>> GetRequestDetails(string requestId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authenticated."
            });

        var response = await _collaborationService.GetRequestDetailsAsync(requestId);
        if (!response.IsSuccessful)
        {
            return BadRequest(response.ErrorResponse);
        }
        return Ok(response);
    }


    [HttpPost("dispute/open/{requestId}")]
    public async Task<ActionResult> OpenDispute(string requestId,[FromBody] OpenDisputeDto disputeDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authenticated."
            });


        var response = await _collaborationService.OpenDisputeAsync(user.Id, requestId, disputeDto);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);

        return Ok(response);
    }


    [HttpPost("send-video/{requestId}")]
    public async Task<IActionResult> SendVideoToUser(string requestId, [FromForm] UploadVideo videoDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authenticated."
            });

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

    [HttpPost("decline-request")]
    public async Task<ActionResult<ServerResponse<List<DeclineResponseDto>>>> DeclineRequest([FromBody] DeclineRequestDto declineRequestDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return new ServerResponse<List<DeclineResponseDto>>
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
            return new ServerResponse<List<DeclineResponseDto>>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "Only Creators are authorized.",
                Data = null!
            };
        }


        var result = await _collaborationService.DeclineRequestAsync(declineRequestDto);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }


    [HttpGet("fetch-disputes")]
    public async Task<IActionResult> FetchDisputesByCreator()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authenticated."
            });
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
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authenticated."
            });

        var response = await _collaborationService.GetDisputeDetailsByIdAsync(disputeId, user.Id);

        if (!response.IsSuccessful)
        {
            return StatusCode(int.Parse(response.ResponseCode), response);
        }

        return Ok(response);
    }


    [HttpPost("resend-request")]
    public async Task<IActionResult> ResendRequest([FromBody] ResendRequestDto resendRequestDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authenticated."
            });

        var result = await _collaborationService.ResendRequestAsync(resendRequestDto);

        if (!result.IsSuccessful)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpPost("cancel-request")]
    public async Task<IActionResult> CloseRequest([FromBody] CloseRequestDto closeRequestDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authenticated."
            });

        var result = await _collaborationService.CloseRequestAsync(closeRequestDto);

        if (!result.IsSuccessful)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpGet("declined-details/{requestId}")]
    public async Task<IActionResult> GetDeclinedDetails(string requestId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authenticated."
            });

        var result = await _collaborationService.GetDeclinedDetailsAsync(requestId);

        if (!result.IsSuccessful)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpPatch("{requestId}/complete")]
    public async Task<IActionResult> MarkRequestAsComplete(string requestId)
    {
        _logger.LogInformation("Attempting to mark request with ID: {RequestId} as complete.", requestId);

        // Validate request exists
        var request = await _repository.GetById<Request>(requestId);
        if (request == null)
        {
            _logger.LogWarning("Request with ID: {RequestId} not found.", requestId);
            return NotFound(new
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Request not found."
            });
        }

        // Check if the request status can be updated to "Complete"
        if (request.Status != RequestStatus.VideoSent)
        {
            _logger.LogWarning("Request with ID: {RequestId} cannot be marked as complete. Current status: {Status}", requestId, request.Status);
            return BadRequest(new
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Request cannot be marked as complete. Status must be 'Video Sent'."
            });
        }

        // Update the request status
        request.Status = RequestStatus.Completed;
        _repository.Update(request);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Request with ID: {RequestId} successfully marked as complete.", requestId);

        return Ok(new
        {
            IsSuccessful = true,
            ResponseCode = "200",
            ResponseMessage = "Request marked as complete successfully."
        });
    }


    [HttpGet("5-completed-creators")]
    public async Task<IActionResult> GetFirstFiveCompletedCollaborations()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "User is not authenticated."
            });
        

        var result = await _collaborationService.GetFirstFiveCompletedCollaborationsAsync();
        //return Ok(new
        //{
        //    IsSuccessful = true,
        //    ResponseCode = "200",
        //    ResponseMessage = "Successfully retrieved completed collaborations.",
        //    Data = collaborations
        //});

        if (!result.IsSuccessful)
            return BadRequest(result);

        return Ok(result);
    }

}
