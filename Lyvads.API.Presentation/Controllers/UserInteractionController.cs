using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Interfaces;
using Lyvads.Application.Dtos.RegularUserDtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Implementions;
using Microsoft.AspNetCore.Identity;
using Lyvads.Domain.Entities;

namespace Lyvads.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserInteractionController : ControllerBase
{
    private readonly IUserInteractionService _userInteractionService;
    private readonly ILogger<UserInteractionController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;


    public UserInteractionController(IUserInteractionService userInteractionService, ILogger<UserInteractionController> logger,
                 UserManager<ApplicationUser> userManager)
    {
        _userInteractionService = userInteractionService;
        _logger = logger;
        _userManager = userManager;
    }

    [HttpPost("Comment")]
    public async Task<IActionResult> AddComment([FromBody] CommentDto commentDto)
    {
        var result = await _userInteractionService.AddCommentAsync(commentDto.UserId, commentDto.Content);

        if (!result.IsSuccess)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpPut("EditComment/{commentId}")]
    public async Task<IActionResult> EditComment(string commentId, [FromBody] string newContent)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _userInteractionService.EditCommentAsync(commentId, user.Id, newContent);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<CommentResponseDto>.Success(result.Data, "Comment updated successfully."));
    }

    [HttpDelete("DeleteComment/{commentId}")]
    public async Task<IActionResult> DeleteComment(string commentId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _userInteractionService.DeleteCommentAsync(commentId, user.Id);

        if (result.IsFailure)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success(null, "Comment deleted successfully."));
    }

    [HttpPost("Like")]
    public async Task<IActionResult> LikeContent([FromBody] LikeDto likeDto)
    {
        var result = await _userInteractionService.LikeContentAsync(likeDto.UserId, likeDto.ContentId);

        if (!result.IsSuccess)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpPost("Unlike")]
    public async Task<IActionResult> UnlikeContent([FromBody] UnlikeDto unlikeDto)
    {
        var result = await _userInteractionService.UnlikeContentAsync(unlikeDto.UserId, unlikeDto.ContentId);

        if (!result.IsSuccess)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success(null, "Content unliked successfully."));
    }

    [HttpPost("FundWallet")]
    public async Task<IActionResult> FundWallet([FromBody] FundWalletDto fundWalletDto)
    {
        var result = await _userInteractionService.FundWalletAsync(fundWalletDto.UserId, fundWalletDto.Amount);

        if (!result.IsSuccess)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    
    [HttpPost("CreateRequest")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateRequestDto createRequestDto)
    {
        _logger.LogInformation("******* Inside the CreateRequest Controller Method ********");

        var result = await _userInteractionService.CreateRequestAsync(createRequestDto);

        if (!result.IsSuccess)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }







    public class UnlikeDto
    {
        public string UserId { get; set; }
        public string ContentId { get; set; }
    }


}