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
using Lyvads.Application.Implementions;

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

    

    [HttpPut("UpdateProfile")]
    public async Task<IActionResult> UpdateCreatorSetUpRates([FromBody] UpdateCreatorProfileDto dto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Call the service to update the creator profile
        var result = await _creatorService.UpdateCreatorSetUpRatesAsync(dto, user.Id);

        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        // Return the updated profile data
        return Ok(ResponseDto<CreatorProfileResponseDto>.Success(result.Data, "Profile updated successfully."));
    }    

    [HttpPost("CreatePost")]
    public async Task<IActionResult> CreatePost([FromBody] PostDto postDto)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.CreatePostAsync(postDto, user.Id);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<PostResponseDto>.Success(result.Data, "Post Successfully Added"));
    }

    [HttpPut("UpdatePost")]
    public async Task<IActionResult> UpdatePost([FromBody] UpdatePostDto postDto)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        // Call the service to update the post
        var result = await _creatorService.UpdatePostAsync(postDto, user.Id);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<PostResponseDto>.Success(result.Data, "Post successfully updated."));
    }

    [HttpDelete("DeletePost/{postId}")]
    public async Task<IActionResult> DeletePost(int postId)
    {
        // Get the logged-in user's ID
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        // Call the service to delete the post
        var result = await _creatorService.DeletePostAsync(postId, user.Id);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success(null, "Post successfully deleted."));
    }

    [HttpPost("Comment")]
    public async Task<IActionResult> CommentOnPost(string postId, string content)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.CommentOnPostAsync(postId, user.Id, content);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<CommentResponseDto>.Success(result.Data, "Comment added successfully."));
    }

    [HttpPost("LikeComment")]
    public async Task<IActionResult> LikeComment(string commentId)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.LikeCommentAsync(commentId, user.Id);
        
        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<LikeResponseDto>.Success(result.Data, "Comment liked successfully."));
    }

    [HttpPost("LikePost")]
    public async Task<IActionResult> LikePost(string postId)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.LikePostAsync(postId, user.Id);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<LikeResponseDto>.Success(result.Data, "Comment liked successfully."));
    }


    [HttpPost("Handle-Request")]
    public async Task<IActionResult> HandleRequest(string requestId, RequestStatus status)
    {
        var result = await _creatorService.HandleRequestAsync(requestId, status);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<RequestResponseDto>.Success(result.Data, "Request handled successfully."));
    }


    [HttpPost("Send-Video")]
    public async Task<IActionResult> SendVideoToUser(string requestId, string videoUrl)
    {
        var result = await _creatorService.SendVideoToUserAsync(requestId, videoUrl);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<VideoResponseDto>.Success(result.Data, "Video sent successfully."));
    }


    [HttpGet("Wallet")]
    public async Task<IActionResult> ViewWalletBalance()
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.ViewWalletBalanceAsync(user.Id);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<WalletBalanceDto>.Success(result.Data, "Wallet balance retrieved successfully."));
    }


    [HttpPost("Withdraw")]
    public async Task<IActionResult> WithdrawToBankAccount([FromBody] WithdrawRequestDto request)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.WithdrawToBankAccountAsync(user.Id, request.Amount, request.Currency);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpGet("Notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.GetNotificationsAsync(user.Id);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<IEnumerable<NotificationResponseDto>>.Success(result.Data, "Notifications retrieved successfully."));
    }


    [HttpGet("Posts")]
    public async Task<IActionResult> GetPostsByCreator()
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.GetPostsByCreatorAsync(user.Id);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<IEnumerable<PostResponseDto>>.Success(result.Data, "Posts retrieved successfully."));
    }


}
