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
using Lyvads.Domain.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Lyvads.Domain.Enums;

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
    public async Task<IActionResult> AddCommentOnPost(string postId, string content)
    {
        var user = await _userManager.GetUserAsync(User);
        // Check if the user is null before proceeding
        if (user == null)
            return Unauthorized("User not found or unauthorized.");

        var result = await _userInteractionService.AddCommentOnPostAsync(postId, user.Id, content);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    [HttpPost("Reply")]
    public async Task<IActionResult> ReplyToComment(string parentCommentId, string content)
    {
        var user = await _userManager.GetUserAsync(User);
        // Check if the user is null before proceeding
        if (user == null)
            return Unauthorized("User not found or unauthorized.");

        var result = await _userInteractionService.ReplyToCommentAsync(parentCommentId, user.Id, content);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    [HttpPut("edit-reply/{replyId}")]
    public async Task<ActionResult<ServerResponse<CommentResponseDto>>> EditReply(
        [FromRoute] string replyId,
        [FromBody] EditReplyRequest request)
    {
        // Assuming the request body contains the user ID and new content.
        var userId = request.UserId;
        var newContent = request.NewContent;

        var response = await _userInteractionService.EditReplyAsync(replyId, userId!, newContent!);

        if (!response.IsSuccessful)
        {
            return StatusCode(int.Parse(response.ResponseCode), response);
        }

        return Ok(response);
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

        return Ok(result);
    }


    [HttpDelete("DeleteComment/{commentId}")]
    public async Task<IActionResult> DeleteComment(string commentId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var result = await _userInteractionService.DeleteCommentAsync(user.Id, commentId);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
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

    [HttpPost("add-creator-to-favorites")]
    public async Task<IActionResult> AddCreatorToFavorites([FromBody] AddFavoriteDto favoriteDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var response = await _userInteractionService.AddCreatorToFavoritesAsync(user.Id, favoriteDto.CreatorId!);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpGet("get-creator-to-favorites")]
    public async Task<IActionResult> AddCreatorToFavorites([FromQuery] string userId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var response = await _userInteractionService.GetFavoriteCreatorsAsync(userId);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpPost("toggle-favorite")]
    public async Task<IActionResult> ToggleFavorite([FromBody] AddFavoriteDto favoriteDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var response = await _userInteractionService.ToggleFavoriteAsync(user.Id, favoriteDto.CreatorId!);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);

        return Ok(response);
    }

    [HttpGet("creators/{creatorId}/posts")]
    public async Task<IActionResult> GetAllPostsOfCreator(string creatorId, [FromQuery] PaginationFilter paginationFilter)
    {
        var response = await _userInteractionService.GetAllPostsOfCreatorAsync(creatorId, paginationFilter);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }


    [HttpGet("creators/featured")]
    public async Task<IActionResult> GetFeaturedCreators([FromQuery] PaginationFilter paginationFilter)
    {
        // Ensure that paginationFilter has default values if not provided in the query
        paginationFilter ??= new PaginationFilter();

        var response = await _userInteractionService.GetFeaturedCreatorsAsync(paginationFilter);

        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);

        return Ok(response);
    }


    [HttpGet("ViewCreatorsProfile/{creatorId}")]
    public async Task<IActionResult> ViewCreatorProfile(string creatorId)
    {
        var response = await _userInteractionService.ViewCreatorProfileAsync(creatorId);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpPost("creators/{creatorId}/toggle-follow-creator")]
    public async Task<IActionResult> FollowCreator(string creatorId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var response = await _userInteractionService.FollowCreatorAsync(user.Id, creatorId);
        if (!response.IsSuccessful)
            return BadRequest(response.ErrorResponse);
        return Ok(response);
    }

    [HttpGet("creators/{creatorId}/is-following")]
    public async Task<IActionResult> IsFollowingCreator(string creatorId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not logged in.");

        var response = await _userInteractionService.CheckIfUserIsFollowingCreatorAsync(user.Id, creatorId);
        return Ok(response);
    }



    [HttpPost("{commentId}/toggle-like-comment")]
    public async Task<IActionResult> ToggleLikeComment(string commentId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized("User not authenticated.");

        var response = await _userInteractionService.ToggleLikeCommentAsync(commentId, userId);

        if (!response.IsSuccessful)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("{postId}/toggle-like-post")]
    public async Task<IActionResult> ToggleLikePost(string postId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("User not authenticated.");
        }

        var response = await _userInteractionService.ToggleLikePostAsync(postId, userId);

        if (!response.IsSuccessful)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("FundWalletViaCardPayment")]
    public async Task<IActionResult> FundWallet([FromBody] FundWalletDto fundWalletDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("User not authenticated.");
        }

        var result = await _userInteractionService.FundWalletAsync(userId, fundWalletDto.Amount,
            fundWalletDto.paymentMethodId!, fundWalletDto.currency);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    [HttpPost("FundWalletViaOnlinePayment")]
    public async Task<IActionResult> FundWalletViaOnlinePayment([FromBody] FundWalletViaOnlinePaymentDto fundWalletDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("User not authenticated.");
        }

        var result = await _userInteractionService.FundWalletViaOnlinePaymentAsync(userId,
            fundWalletDto.Amount, fundWalletDto.PaymentMethodId, fundWalletDto.currency);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }

    //[HttpPost("ConfirmPayment")]
    //public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentDto confirmPaymentDto)
    //{
    //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //    if (userId == null)
    //    {
    //        return Unauthorized("User not authenticated.");
    //    }

    //    var result = await _userInteractionService.ConfirmPaymentAsync(confirmPaymentDto.PaymentIntentId, userId, confirmPaymentDto.Amount);

    //    if (!result.IsSuccessful)
    //        return BadRequest(result.ErrorResponse);

    //    return Ok(result);
    //}


    [HttpPost("MakeRequest")]
    public async Task<IActionResult> MakeRequest([FromQuery] string creatorId, [FromQuery] AppPaymentMethod payment, 
        [FromBody] CreateRequestDto createRequestDto)
    {
        if (createRequestDto == null || string.IsNullOrEmpty(creatorId))
            return BadRequest("Invalid request data");

        var result = await _userInteractionService.MakeRequestAsync(creatorId, payment, createRequestDto);

        if (!result.IsSuccessful)
            return BadRequest(result.ErrorResponse);

        return Ok(result);
    }


    //[HttpGet("favorite-creators")]
    //public async Task<IActionResult> GetFavoriteCreators()
    //{
    //    var user = await _userManager.GetUserAsync(User);
    //    if (user == null)
    //        return Unauthorized("User not logged in.");

    //    var response = await _userInteractionService.GetFavoriteCreatorsAsync(user.Id);
    //    if (!response.IsSuccessful)
    //        return BadRequest(response.ErrorResponse);
    //    return Ok(response);
    //}

    //[HttpPost("remove-creator-from-favorites")]
    //public async Task<IActionResult> RemoveCreatorFromFavorites([FromBody] RemoveFavoriteDto favoriteDto)
    //{
    //    // Get the logged-in user's ID
    //    var user = await _userManager.GetUserAsync(User);
    //    if (user == null)
    //        return Unauthorized("User not logged in.");

    //    // Call the service to remove the creator from the user's favorites
    //    var response = await _userInteractionService.RemoveCreatorFromFavoritesAsync(user.Id, favoriteDto.CreatorId!);
    //    if (!response.IsSuccessful)
    //        return BadRequest(response.ErrorResponse);

    //    return Ok(response);
    //}


    //[HttpPost("creators/{creatorId}/unfollow")]
    //public async Task<IActionResult> UnfollowCreator(string userId, string creatorId)
    //{
    //    var response = await _userInteractionService.UnfollowCreatorAsync(userId, creatorId);
    //    if (!response.IsSuccessful)
    //        return BadRequest(response.ErrorResponse);
    //    return Ok(response);
    //}

    //[HttpPost("CreateRequest")]
    //public async Task<IActionResult> CreateRequest([FromBody] CreateRequestDto createRequestDto)
    //{
    //    _logger.LogInformation("******* Inside the CreateRequest Controller Method ********");

    //    var result = await _userInteractionService.CreateRequestAsync(createRequestDto);

    //    if (!result.IsSuccess)
    //        return BadRequest(ResponseDto<object>.Failure(result.Errors));

    //    return Ok(ResponseDto<object>.Success());
    //}
    //[HttpPost("Like")]
    //public async Task<IActionResult> LikeContent([FromBody] LikeDto likeDto)
    //{
    //    try
    //    {
    //        var result = await _userInteractionService.LikeContentAsync(likeDto.UserId!, likeDto.ContentId!);

    //        if (!result.IsSuccessful)
    //            return BadRequest(result.ErrorResponse);

    //        return Ok(result);
    //    }
    //    catch (DbUpdateException ex)
    //    {
    //        // Log the inner exception
    //        var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
    //        return StatusCode(500, new { Message = "An error occurred while saving changes.", Error = innerExceptionMessage });
    //    }

    //}


    //[HttpPost("Unlike")]
    //public async Task<IActionResult> UnlikeContent([FromBody] UnlikeDto unlikeDto)
    //{
    //    _logger.LogInformation("******* Verifying Email Update ********");

    //    if (string.IsNullOrEmpty(unlikeDto.UserId))
    //    {
    //        return BadRequest("User ID cannot be null or empty");
    //    }

    //    if (string.IsNullOrEmpty(unlikeDto.ContentId))
    //    {
    //        return BadRequest("Content ID cannot be null or empty");
    //    }

    //    var result = await _userInteractionService.UnlikeContentAsync(unlikeDto.UserId, unlikeDto.ContentId);

    //    if (!result.IsSuccessful)
    //        return BadRequest(result.ErrorResponse);

    //    return Ok(result);
    //}

    public class FundWalletViaOnlinePaymentDto
    {
        public decimal Amount { get; set; } = default!;
        public string PaymentMethodId { get; set; } = default!;
        public string currency { get; set; } = default!;

    }

    public class ConfirmPaymentDto
    {
        public decimal Amount { get; set; } = default!;
        public string PaymentIntentId { get; set; } = default!;

    }

    public class RemoveFavoriteDto
    {
        public string CreatorId { get; set; } = default!;
    }
 
    public class UnlikeDto
    {
        public string? UserId { get; set; }
        public string? ContentId { get; set; }
    }

    public class AddFavoriteDto
    {
        public string? CreatorId { get; set; }
    }

    public class EditReplyRequest
    {
        public string? UserId { get; set; }
        public string? NewContent { get; set; }
    }
}