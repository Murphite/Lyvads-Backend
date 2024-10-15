
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Domain.Responses;
using Lyvads.Shared.DTOs;

namespace Lyvads.Application.Interfaces;

public interface IUserInteractionService
{
    Task<ServerResponse<string>> MakeRequestAsync(CreateRequestDto createRequestDto);
    Task<ServerResponse<CommentResponseDto>> EditCommentAsync(string commentId, string userId, string newContent);
    Task<ServerResponse<CommentResponseDto>> AddCommentOnPostAsync(string postId, string userId, string content);
    Task<ServerResponse<CommentResponseDto>> ReplyToCommentAsync(string parentCommentId, string userId, string content);
    Task<ServerResponse<CommentResponseDto>> EditReplyAsync(string replyId, string userId, string newContent);
    Task<ServerResponse<object>> DeleteCommentAsync(string userId, string commentId);
    Task<ServerResponse<object>> LikeContentAsync(string userId, string contentId);
    Task<ServerResponse<object>> UnlikeContentAsync(string userId, string contentId);
    Task<ServerResponse<object>> FundWalletAsync(string userId, decimal amount);
    Task<ServerResponse<int>> GetNumberOfLikesAsync(string postId);
    Task<ServerResponse<int>> GetNumberOfCommentsAsync(string postId);
    Task<ServerResponse<List<string>>> GetUsersWhoLikedPostAsync(string postId);
    Task<ServerResponse<List<CommentResponseDto>>> GetAllCommentsOnPostAsync(string postId);
    Task<ServerResponse<object>> AddCreatorToFavoritesAsync(string userId, string creatorId);
    Task<ServerResponse<List<CreatorResponseDto>>> GetFavoriteCreatorsAsync(string userId);
    Task<ServerResponse<List<ViewPostResponseDto>>> GetAllPostsOfCreatorAsync(string creatorId);
    Task<ServerResponse<List<FeaturedCreatorDto>>> GetFeaturedCreatorsAsync(int count);
    Task<ServerResponse<CreatorProfileDto>> ViewCreatorProfileAsync(string creatorId);
    Task<ServerResponse<object>> FollowCreatorAsync(string userId, string creatorId);
    Task<ServerResponse<object>> UnfollowCreatorAsync(string userId, string creatorId);

    //Task<ServerResponse<object>> CreateRequestAsync(CreateRequestDto createRequestDto);
}
