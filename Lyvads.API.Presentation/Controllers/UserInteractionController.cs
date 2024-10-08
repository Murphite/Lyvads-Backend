using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Interfaces;
using Lyvads.Application.Dtos.RegularUserDtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Implementations;
using Microsoft.AspNetCore.Identity;
using Lyvads.Domain.Entities;
using Lyvads.Shared.DTOs;
using Lyvads.Application.Dtos;

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

        if(!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(ResponseDto<object>.Success(result.Data));
    }


    [HttpPut("EditComment/{commentId}")]
    public async Task<IActionResult> EditComment(string commentId, [FromBody] string newContent)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _userInteractionService.EditCommentAsync(commentId, user.Id, newContent);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(ResponseDto<CommentResponseDto>.Success(result.Data, "Comment updated successfully."));
    }


    [HttpDelete("DeleteComment/{commentId}")]
    public async Task<IActionResult> DeleteComment(string commentId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _userInteractionService.DeleteCommentAsync(commentId, user.Id);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(ResponseDto<object>.Success(result.Data, "Comment deleted successfully."));
    }


    [HttpPost("Like")]
    public async Task<IActionResult> LikeContent([FromBody] LikeDto likeDto)
    {
        var result = await _userInteractionService.LikeContentAsync(likeDto.UserId, likeDto.ContentId);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(ResponseDto<object>.Success(result.Data));
    }


    [HttpPost("Unlike")]
    public async Task<IActionResult> UnlikeContent([FromBody] UnlikeDto unlikeDto)
    {
        _logger.LogInformation("******* Verifying Email Update ********");

        if (string.IsNullOrEmpty(unlikeDto.UserId))
        {
            return BadRequest("User ID cannot be null or empty");
        }

        if (string.IsNullOrEmpty(unlikeDto.ContentId))
        {
            return BadRequest("Content ID cannot be null or empty");
        }

        var result = await _userInteractionService.UnlikeContentAsync(unlikeDto.UserId, unlikeDto.ContentId);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(ResponseDto<object>.Success(result.Data, "Content unliked successfully."));
    }



    [HttpPost("FundWallet")]
    public async Task<IActionResult> FundWallet([FromBody] FundWalletDto fundWalletDto)
    {
        var result = await _userInteractionService.FundWalletAsync(fundWalletDto.UserId, fundWalletDto.Amount);

        if(!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(ResponseDto<object>.Success(result.Data));
    }   

   
    [HttpPost("MakeRequest")]
    public async Task<IActionResult> MakeRequest([FromQuery] string creatorId, [FromBody] CreateRequestDto createRequestDto)
    {
        if (createRequestDto == null || string.IsNullOrEmpty(creatorId))
            return BadRequest("Invalid request data");

        createRequestDto.CreatorId = creatorId;
        var result = await _userInteractionService.MakeRequestAsync(createRequestDto);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(ResponseDto<MakeRequestResponseDto>.Success(result.Data));
    }

    [HttpGet("posts/{postId}/likes")]
    public async Task<IActionResult> GetNumberOfLikes(string postId)
    {
        var response = await _userInteractionService.GetNumberOfLikesAsync(postId);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpGet("posts/{postId}/comments")]
    public async Task<IActionResult> GetNumberOfComments(string postId)
    {
        var response = await _userInteractionService.GetNumberOfCommentsAsync(postId);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpGet("posts/{postId}/users-who-liked")]
    public async Task<IActionResult> GetUsersWhoLikedPost(string postId)
    {
        var response = await _userInteractionService.GetUsersWhoLikedPostAsync(postId);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpGet("posts/{postId}/comments/all")]
    public async Task<IActionResult> GetAllCommentsOnPost(string postId)
    {
        var response = await _userInteractionService.GetAllCommentsOnPostAsync(postId);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpPost("favorites")]
    public async Task<IActionResult> AddCreatorToFavorites([FromBody] AddFavoriteDto favoriteDto)
    {
        var response = await _userInteractionService.AddCreatorToFavoritesAsync(favoriteDto.UserId, favoriteDto.CreatorId);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpGet("{userId}/favorites/creators")]
    public async Task<IActionResult> GetFavoriteCreators(string userId)
    {
        var response = await _userInteractionService.GetFavoriteCreatorsAsync(userId);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpGet("creators/{creatorId}/posts")]
    public async Task<IActionResult> GetAllPostsOfCreator(string creatorId)
    {
        var response = await _userInteractionService.GetAllPostsOfCreatorAsync(creatorId);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpGet("creators/featured")]
    public async Task<IActionResult> GetFeaturedCreators([FromQuery] int count)
    {
        var response = await _userInteractionService.GetFeaturedCreatorsAsync(count);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpGet("creators/{creatorId}")]
    public async Task<IActionResult> ViewCreatorProfile(string creatorId)
    {
        var response = await _userInteractionService.ViewCreatorProfileAsync(creatorId);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpPost("creators/{creatorId}/follow")]
    public async Task<IActionResult> FollowCreator(string userId, string creatorId)
    {
        var response = await _userInteractionService.FollowCreatorAsync(userId, creatorId);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpPost("creators/{creatorId}/unfollow")]
    public async Task<IActionResult> UnfollowCreator(string userId, string creatorId)
    {
        var response = await _userInteractionService.UnfollowCreatorAsync(userId, creatorId);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    //[HttpPost("CreateRequest")]
    //public async Task<IActionResult> CreateRequest([FromBody] CreateRequestDto createRequestDto)
    //{
    //    _logger.LogInformation("******* Inside the CreateRequest Controller Method ********");

    //    var result = await _userInteractionService.CreateRequestAsync(createRequestDto);

    //    if (!result.IsSuccess)
    //        return BadRequest(ResponseDto<object>.Failure(result.Errors));

    //    return Ok(ResponseDto<object>.Success());
    //}


    public class UnlikeDto
    {
        public string? UserId { get; set; }
        public string? ContentId { get; set; }
    }

    public class AddFavoriteDto
    {
        public string? UserId { get; set; }
        public string? CreatorId { get; set; }
    }
}