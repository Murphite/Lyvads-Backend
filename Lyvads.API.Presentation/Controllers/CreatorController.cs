using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Lyvads.Application.Interfaces;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.API.Presentation.Dtos;

namespace Lyvads.API.Presentation.Controllers;

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

    [HttpPost("CreatePost")]
    public async Task<IActionResult> CreatePost([FromBody] PostDto postDto)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.CreatePostAsync(postDto, user.Id);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpPost("comment")]
    public async Task<IActionResult> CommentOnPost(string postId, string content)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.CommentOnPostAsync(postId, user.Id, content);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpPost("like")]
    public async Task<IActionResult> LikeComment(string commentId)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.LikeCommentAsync(commentId, user.Id);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpPost("handle-request")]
    public async Task<IActionResult> HandleRequest(string requestId, RequestStatus status)
    {
        var result = await _creatorService.HandleRequestAsync(requestId, status);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpPost("send-video")]
    public async Task<IActionResult> SendVideoToUser(string requestId, string videoUrl)
    {
        var result = await _creatorService.SendVideoToUserAsync(requestId, videoUrl);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpGet("wallet")]
    public async Task<IActionResult> ViewWalletBalance()
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.ViewWalletBalanceAsync(user.Id);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> WithdrawToBankAccount([FromBody] WithdrawRequestDto request)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.WithdrawToBankAccountAsync(user.Id, request.Amount, request.Currency);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.GetNotificationsAsync(user.Id);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpGet("posts")]
    public async Task<IActionResult> GetPostsByCreator()
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _creatorService.GetPostsByCreatorAsync(user.Id);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }
}
