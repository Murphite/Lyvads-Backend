
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Responses;
using Lyvads.Shared.DTOs;

namespace Lyvads.Application.Interfaces;

public interface IUserInteractionService
{
    Task<ServerResponse<bool>> CheckIfCreatorIsInUserFavoritesAsync(string userId, string creatorId);
    Task<ServerResponse<WalletBalanceDto>> ViewWalletBalanceAsync(string userId);
    Task<ServerResponse<int>> GetUsersFollowingCreatorCountAsync(string creatorId);
    Task<ServerResponse<List<UserFollowerDto>>> GetUsersFollowingCreatorDetailsAsync(string creatorId);
    Task<ServerResponse<int>> GetCreatorsFollowingCountAsync(string userId);
    List<ChargeAmountDto> GetChargeDetails(int totalAmount, List<Charge> charges, CreateRequestDto requestDto);
    Task<ServerResponse<MakeRequestDetailsDto>> MakeRequestAsync(string creatorId, AppPaymentMethod payment,
       CreateRequestDto createRequestDto);
    Task<ServerResponse<CommentResponseDto>> EditCommentAsync(string commentId, string userId, string newContent);
    Task<ServerResponse<CommentResponseDto>> AddCommentOnPostAsync(string postId, string userId, string content);
    Task<ServerResponse<CommentResponseDto>> ReplyToCommentAsync(string parentCommentId, string userId, string content);
    Task<ServerResponse<CommentResponseDto>> EditReplyAsync(string replyId, string userId, string newContent);
    Task<ServerResponse<object>> DeleteCommentAsync(string userId, string commentId);
    //Task<ServerResponse<object>> LikeContentAsync(string userId, string contentId);
   // Task<ServerResponse<object>> UnlikeContentAsync(string userId, string contentId);
   // Task<ServerResponse<object>> FundWalletAsync(string userId, decimal amount, string paymentMethodId, string currency);
    //Task<ServerResponse<string>> FundWalletViaOnlinePaymentAsync(string userId, decimal amount, string paymentMethodId, string currency);
    Task<ServerResponse<object>> ConfirmPaymentAsync(string paymentIntentId, string userId, decimal amount);
    Task<ServerResponse<int>> GetNumberOfLikesAsync(string postId);
    Task<ServerResponse<int>> GetNumberOfCommentsAsync(string postId);
    Task<ServerResponse<List<string>>> GetUsersWhoLikedPostAsync(string postId);
    Task<ServerResponse<List<CommentResponseDto>>> GetAllCommentsOnPostAsync(string postId);
    Task<ServerResponse<object>> AddCreatorToFavoritesAsync(string userId, string creatorId);
    Task<ServerResponse<object>> RemoveCreatorFromFavoritesAsync(string userId, string creatorId);
    Task<ServerResponse<object>> ToggleFavoriteAsync(string userId, string creatorId);
    Task<ServerResponse<List<CreatorResponseDto>>> GetFavoriteCreatorsAsync(string userId);
    Task<ServerResponse<List<ViewPostResponseDto>>> GetAllPostsOfCreatorAsync(string creatorId);
    Task<ServerResponse<PaginatorDto<IEnumerable<FeaturedCreatorDto>>>> GetFeaturedCreatorsAsync(PaginationFilter paginationFilter);
    Task<ServerResponse<CreatorProfileDto>> ViewCreatorProfileAsync(string creatorId);
    Task<ServerResponse<object>> FollowCreatorAsync(string userId, string creatorId);
    Task<ServerResponse<object>> UnfollowCreatorAsync(string userId, string creatorId);
    Task<ServerResponse<LikeResponseDto>> ToggleLikePostAsync(string postId, string userId);
    Task<ServerResponse<LikeResponseDto>> ToggleLikeCommentAsync(string commentId, string userId);
    Task<ServerResponse<PaginatorDto<IEnumerable<ViewPostResponseDto>>>> GetAllPostsOfCreatorAsync(string creatorId, PaginationFilter paginationFilter);
    //Task<ServerResponse<object>> CreateRequestAsync(CreateRequestDto createRequestDto);
    Task<ServerResponse<bool>> CheckIfUserIsFollowingCreatorAsync(string userId, string creatorId);
}
