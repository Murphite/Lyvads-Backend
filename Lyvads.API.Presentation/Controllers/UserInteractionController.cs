using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Interfaces;
using Lyvads.Application.Dtos.RegularUserDtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Lyvads.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserInteractionController : ControllerBase
{
    private readonly IUserInteractionService _userInteractionService;

    public UserInteractionController(IUserInteractionService userInteractionService)
    {
        _userInteractionService = userInteractionService;
    }

    [HttpPost("Comment")]
    public async Task<IActionResult> AddComment([FromBody] CommentDto commentDto)
    {
        var result = await _userInteractionService.AddCommentAsync(commentDto.UserId, commentDto.Content);

        if (!result.IsSuccess)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpPost("Like")]
    public async Task<IActionResult> LikeContent([FromBody] LikeDto likeDto)
    {
        var result = await _userInteractionService.LikeContentAsync(likeDto.UserId, likeDto.ContentId);

        if (!result.IsSuccess)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }

    [HttpPost("FundWallet")]
    public async Task<IActionResult> FundWallet([FromBody] FundWalletDto fundWalletDto)
    {
        var result = await _userInteractionService.FundWalletAsync(fundWalletDto.UserId, fundWalletDto.Amount);

        if (!result.IsSuccess)
            return BadRequest(ResponseDto<object>.Failure(result.Errors));

        return Ok(ResponseDto<object>.Success());
    }
}