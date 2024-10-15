using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Lyvads.Application.Interfaces;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.API.Presentation.Dtos;
using Microsoft.AspNetCore.Authorization;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Implementations;
using Lyvads.Domain.Responses;
using Lyvads.Application.Dtos;
using System.Security.Claims;
using Lyvads.Shared.DTOs;
using Azure;

namespace Lyvads.API.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CreatorController : ControllerBase
{
    private readonly ICreatorService _creatorService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CreatorController> _logger;

    public CreatorController(ICreatorService creatorService, UserManager<ApplicationUser> userManager, ILogger<CreatorController> logger)
    {
        _creatorService = creatorService;
        _userManager = userManager;
        _logger = logger;
    }

    

    [HttpPut("update-creator-setUpRates")]
    public async Task<IActionResult> UpdateCreatorSetUpRates([FromBody] UpdateCreatorProfileDto dto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Call the service to update the creator profile
        var result = await _creatorService.UpdateCreatorSetUpRatesAsync(dto, user.Id);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);


        // Return the updated profile data
        return Ok(ResponseDto<CreatorProfileResponseDto>.Success(result.Data, "Profile updated successfully."));
    }

    [HttpPost("CreatePost")]
    [Authorize(Roles = "Creator")]
    public async Task<IActionResult> CreatePost([FromForm] PostDto postDto, [FromQuery] PostVisibility visibility,
        [FromForm] UploadImage photo)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ServerResponse<PostResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "Unauthorized. User ID not found."
            });
        }

        // Validate the file (photo) if provided
        if (photo.Image != null && !IsValidFile(photo.Image))  // Validate using the Image property
        {
            return BadRequest(new { message = "Invalid File Extension" });
        }

        // Convert IFormFile to byte array
        byte[] fileBytes = null!;
        if (photo.Image != null)
        {
            using (var stream = new MemoryStream())
            {
                await photo.Image.CopyToAsync(stream);  // Use the Image property for file operations
                fileBytes = stream.ToArray();
            }
        }

        // Call the service with the Image file and other post details
        var response = await _creatorService.CreatePostAsync(postDto, visibility, userId, photo.Image!);

        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);

        return Ok(response);
    }


    [HttpPut("update-post/{postId}")]
    [Authorize(Roles = "Creator")]
    public async Task<IActionResult> UpdatePost(string postId, [FromForm] UpdatePostDto postDto, [FromQuery] PostVisibility visibility,
        [FromForm] UploadImage photo)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get user ID from claims

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ServerResponse<PostResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "Unauthorized. User ID not found."
            });
        }

        // Set PostId in DTO to match route parameter
        // postDto.PostId = postId; // No longer needed

        // Validate the file (photo) if provided
        if (photo.Image != null && !IsValidFile(photo.Image))  // Validate using the Image property
        {
            return BadRequest(new { message = "Invalid File Extension" });
        }

        // Convert IFormFile to byte array
        byte[] fileBytes = null!;
        if (photo.Image != null)
        {
            using (var stream = new MemoryStream())
            {
                await photo.Image.CopyToAsync(stream);  // Use the Image property for file operations
                fileBytes = stream.ToArray();
            }
        }

        // Call the service with the Image file and other post details
        var response = await _creatorService.UpdatePostAsync(postId, postDto, visibility, userId, photo.Image!); // Pass postId directly

        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);

        return Ok(response);
    }


    private bool IsValidFile(IFormFile file)
    {
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        return validExtensions.Contains(extension);
    }


    [HttpDelete("deletePost/{postId}")]
    public async Task<IActionResult> DeletePost(string postId)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        // Call the service to delete the post
        var result = await _creatorService.DeletePostAsync(postId, user.Id);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    
    [HttpPost("commentOnPost")]
    public async Task<IActionResult> CommentOnPost(string postId, string content)
    {
        var user = await _userManager.GetUserAsync(User);
        // Check if the user is null before proceeding
        if (user == null)
            return Unauthorized("User not found or unauthorized.");

        var result = await _creatorService.CommentOnPostAsync(postId, user.Id, content);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    
    [HttpPost("likeComment")]
    public async Task<IActionResult> LikeComment(string commentId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found or unauthorized.");
        var result = await _creatorService.LikeCommentAsync(commentId, user.Id);
        
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);


        return Ok(result);
    }


    [HttpPost("likePost")]
    public async Task<IActionResult> LikePost(string postId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found or unauthorized.");
        var result = await _creatorService.LikePostAsync(postId, user.Id);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(ResponseDto<LikeResponseDto>.Success(result.Data, "Comment liked successfully."));
    }


    [HttpGet("posts")]
    public async Task<IActionResult> GetPostsByCreator()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found or unauthorized.");

        var result = await _creatorService.GetPostsByCreatorAsync(user.Id);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }


    [HttpGet("searchQuery")]
    public async Task<ActionResult<ServerResponse<List<FilterCreatorDto>>>> SearchCreators(
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? location,
            [FromQuery] string? industry)
    {
        var result = await _creatorService.SearchCreatorsAsync(minPrice, maxPrice, location, industry);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    
    [HttpPost("send-video")]
    public async Task<IActionResult> SendVideoToUser(string requestId, [FromForm] UploadVideo videoDto)
    {
        // Check if the video is provided
        if (videoDto.Video == null)
        {
            return BadRequest(new { message = "No video file provided." });
        }

        // Validate the file (video) if provided
        if (!IsValidFile(videoDto.Video)) // Use the same validation logic as for images
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
        var result = await _creatorService.SendVideoToUserAsync(requestId, videoDto.Video); // Pass the byte array

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }


    [HttpGet("wallet-balance")]
    public async Task<IActionResult> ViewWalletBalance()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found or unauthorized.");
        var result = await _creatorService.ViewWalletBalanceAsync(user.Id);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }


    [HttpPost("handle-request")]
    public async Task<IActionResult> HandleRequest(string requestId, RequestStatus status)
    {
        var result = await _creatorService.HandleRequestAsync(requestId, status);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }


    [HttpPost("withdraw-to-bankAccount")]
    public async Task<IActionResult> WithdrawToBankAccount([FromBody] WithdrawRequestDto request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found or unauthorized.");
        var result = await _creatorService.WithdrawToBankAccountAsync(user.Id, request.Amount, request.Currency);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found or unauthorized.");

        var result = await _creatorService.GetNotificationsAsync(user.Id);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }


    

}
