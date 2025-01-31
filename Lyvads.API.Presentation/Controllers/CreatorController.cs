﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Lyvads.Application.Interfaces;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Lyvads.Domain.Responses;
using Lyvads.Shared.DTOs;
using System.Security.Claims;

namespace Lyvads.API.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CreatorController : ControllerBase
{
    private readonly ICreatorService _creatorService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CreatorController> _logger;
    private readonly IMediaService _mediaService;

    public CreatorController(ICreatorService creatorService, UserManager<ApplicationUser> userManager,
        ILogger<CreatorController> logger, IMediaService mediaService)
    {
        _creatorService = creatorService;
        _userManager = userManager;
        _logger = logger;
        _mediaService = mediaService;
    }
        

    [HttpPost("update-creator-rates")]
    public async Task<IActionResult> UpdateCreatorRates([FromBody] UpdateCreatorRateDto dto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Call the service to update the creator profile
        var result = await _creatorService.UpdateCreatorRatesAsync(dto, user.Id);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    [HttpDelete("delete-creator-rate")]
    public async Task<IActionResult> DeleteCreatorRates([FromQuery] string rateId)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Call the service to update the creator profile
        var result = await _creatorService.DeleteCreatorRateAsync(rateId, user.Id);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    [HttpPost("CreatePost")]
    //[Authorize(Roles = "Creator")]
    public async Task<IActionResult> CreatePost([FromForm] PostDto postDto, [FromQuery] PostVisibility visibility,
    [FromForm] List<IFormFile> mediaFiles)
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

        var response = await _creatorService.CreatePostAsync(postDto, visibility, userId, mediaFiles);

        if (!response.IsSuccessful)
            return BadRequest(response);

        return Ok(response);
    }


    [HttpPost("update-post/{postId}")]
    //[Authorize(Roles = "Creator")]
    public async Task<IActionResult> UpdatePost(string postId, [FromForm] UpdatePostDto postDto,
    [FromQuery] PostVisibility visibility, [FromForm] List<IFormFile> mediaFiles)
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

        // Call the service with multiple media files
        var response = await _creatorService.UpdatePostAsync(postId, postDto, visibility, userId, mediaFiles);

        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);

        return Ok(response);
    }

   
    [HttpDelete("delete-media")]
    public async Task<IActionResult> DeleteMediaFromPost([FromBody] DeleteMediaRequestDto request)
    {
        if (string.IsNullOrEmpty(request.PostId) || request.MediaToDelete == null || !request.MediaToDelete.Any())
        {
            return BadRequest(new
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "PostId and MediaToDelete are required."
            });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new
            {
                IsSuccessful = false,
                ResponseCode = "401",
                ResponseMessage = "Unauthorized request."
            });
        }

        var editDto = new PostEditDto
        {
            MediaToDelete = request.MediaToDelete
        };

        var response = await _creatorService.DeleteMediaPostAsync(request.PostId, user.Id, editDto);

        if (response.IsSuccessful)
        {
            return Ok(response);
        }

        return StatusCode(response.ResponseCode == "404" ? 404 : 400, response);
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

    
    [HttpPost("CommentOnPost")]
    public async Task<IActionResult> CommentOnPost(CommentOnPostDto commentOnPost)
    {
        var user = await _userManager.GetUserAsync(User);
        // Check if the user is null before proceeding
        if (user == null)
            return Unauthorized("User not found or unauthorized.");

        var result = await _creatorService.CommentOnPostAsync(commentOnPost.postId, user.Id, commentOnPost.content);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }


    [HttpGet("Posts")]
    public async Task<IActionResult> GetPostsByCreator([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        // Get the logged-in user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found or unauthorized.");

        // Create a pagination filter using the query parameters
        var paginationFilter = new PaginationFilter(pageNumber, pageSize);

        // Call the service method
        var result = await _creatorService.GetPostsByCreatorAsync(user.Id, paginationFilter);

        // Check the result and return the appropriate response
        if (!result.IsSuccessful)
            return BadRequest(new { result.ResponseCode, result.ResponseMessage });

        return Ok(new { result.ResponseCode, result.ResponseMessage, Data = result.Data });
    }



    [HttpGet("searchQuery")]
    public async Task<ActionResult<ServerResponse<PaginatorDto<IEnumerable<FilterCreatorDto>>>>> SearchCreators(
    [FromQuery] decimal? minPrice,
    [FromQuery] decimal? maxPrice,
    [FromQuery] string? location,
    [FromQuery] string? industry,
    [FromQuery] string? keyword,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
    {
        var paginationFilter = new PaginationFilter
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _creatorService.SearchCreatorsAsync(minPrice, maxPrice, location, industry, keyword, paginationFilter);

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


    [HttpPost("WithdrawFundsToBankAccount")]
    public async Task<IActionResult> WithdrawFunds([FromBody] WithdrawFundsDto withdrawFundsDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("User not authenticated.");
        }

        var result = await _creatorService.WithdrawFundsToBankAccountAsync(userId, withdrawFundsDto.Amount, withdrawFundsDto.BankCardId);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }


    [HttpGet("posts/{postId}/isLiked")]
    public async Task<IActionResult> IsPostLikedByUser(string postId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found or unauthorized.");

        var result = await _creatorService.IsPostLikedByUserAsync(postId, user.Id);
        return Ok(result);
    }

    [HttpGet("comments/{commentId}/isLiked")]
    public async Task<IActionResult> IsCommentLikedByUser(string commentId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found or unauthorized.");

        var result = await _creatorService.IsCommentLikedByUserAsync(commentId, user.Id);
        return Ok(result);
    }

    [HttpGet("posts/{postId}/comments")]
    public async Task<IActionResult> GetCommentsByPostId(string postId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found or unauthorized.");

        var paginationFilter = new PaginationFilter(pageNumber, pageSize);
        var result = await _creatorService.GetCommentsByPostIdAsync(postId, paginationFilter);

        if (!result.IsSuccessful)
            return BadRequest(new { result.ResponseCode, result.ResponseMessage });

        return Ok(new { result.ResponseCode, result.ResponseMessage, Data = result.Data });
    }



    public class WithdrawFundsDto
    {
        public decimal Amount { get; set; }
        public string BankCardId { get; set; } = string.Empty; // Assuming this is a required field
    }

    //[HttpPost("withdraw-to-bankAccount")]
    //public async Task<IActionResult> WithdrawToBankAccount([FromBody] WithdrawRequestDto request)
    //{
    //    var user = await _userManager.GetUserAsync(User);
    //    if (user == null)
    //        return Unauthorized("User not found or unauthorized.");
    //    var result = await _creatorService.WithdrawToBankAccountAsync(user.Id, request.Amount, request.Currency);

    //    if (!result.IsSuccessful)
    //        return BadRequest(result.ErrorResponse);

    //    return Ok(result);
    //}

    //[HttpPost("send-video")]
    //public async Task<IActionResult> SendVideoToUser(string requestId, [FromForm] UploadVideo videoDto)
    //{
    //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //    if (userId == null)
    //    {
    //        return Unauthorized("User not authenticated.");
    //    }

    //    // Check if the video is provided
    //    if (videoDto.Video == null)
    //    {
    //        return BadRequest(new { message = "No video file provided." });
    //    }

    //    // Validate the file (video) if provided
    //    if (!IsValidVideoFile(videoDto.Video)) // Use the same validation logic as for images
    //    {
    //        return BadRequest(new { message = "Invalid File Extension" });
    //    }

    //    // Convert IFormFile to byte array
    //    byte[] videoBytes;
    //    using (var stream = new MemoryStream())
    //    {
    //        await videoDto.Video.CopyToAsync(stream);
    //        videoBytes = stream.ToArray();
    //    }

    //    // Call the service with the video file and other post details
    //    var result = await _creatorService.SendVideoToUserAsync(requestId, videoDto.Video); // Pass the byte array

    //    if (!result.IsSuccessful)
    //        return BadRequest(result.ErrorResponse);

    //    return Ok(result);
    //}

    //private bool IsValidVideoFile(IFormFile file)
    //{
    //    var validExtensions = new[] { ".mp4", ".avi", ".mov" };
    //    var extension = Path.GetExtension(file.FileName).ToLower();
    //    return validExtensions.Contains(extension);
    //}


    //[HttpPost("likeComment")]
    //public async Task<IActionResult> LikeComment(string commentId)
    //{
    //    var user = await _userManager.GetUserAsync(User);
    //    if (user == null)
    //        return Unauthorized("User not found or unauthorized.");
    //    var result = await _creatorService.LikeCommentAsync(commentId, user.Id);

    //    if (!result.IsSuccessful)
    //        return BadRequest(result.ErrorResponse);


    //    return Ok(result);
    //}


    //[HttpPost("likePost")]
    //public async Task<IActionResult> LikePost(string postId)
    //{
    //    var user = await _userManager.GetUserAsync(User);
    //    if (user == null)
    //        return Unauthorized("User not found or unauthorized.");
    //    var result = await _creatorService.LikePostAsync(postId, user.Id);

    //    if (!result.IsSuccessful)
    //        return BadRequest(result.ErrorResponse);

    //    return Ok(ResponseDto<LikeResponseDto>.Success(result.Data, "Comment liked successfully."));
    //}


    //[HttpPut("update-creator-setUpRates")]
    //public async Task<IActionResult> UpdateCreatorSetUpRates([FromBody] UpdateCreatorProfileDto dto)
    //{
    //    // Get the logged-in user's ID
    //    var user = await _userManager.GetUserAsync(User);
    //    if (user == null)
    //        return NotFound("User not found.");

    //    // Call the service to update the creator profile
    //    var result = await _creatorService.UpdateCreatorSetUpRatesAsync(dto, user.Id);

    //    if (!result.IsSuccessful)
    //        return BadRequest(result.ErrorResponse);


    //    // Return the updated profile data
    //    return Ok(ResponseDto<CreatorProfileResponseDto>.Success(result.Data, "Profile updated successfully."));
    //}

}
