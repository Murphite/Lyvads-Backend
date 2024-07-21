using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos;
using Lyvads.Domain.Enums;


namespace Lyvads.Application.Interfaces;

public interface ICreatorService
{
    Task<Result> CreatePostAsync(PostDto postDto, string creatorId);
    Task<Result> CommentOnPostAsync(string postId, string userId, string content);
    Task<Result> LikeCommentAsync(string commentId, string userId);
    Task<Result> HandleRequestAsync(string requestId, RequestStatus status);
    Task<Result> SendVideoToUserAsync(string requestId, string videoUrl);
    Task<Result> ViewWalletBalanceAsync(string creatorId);
    Task<Result> WithdrawToBankAccountAsync(string creatorId, decimal amount, string currency);
    Task<Result> GetNotificationsAsync(string creatorId);
    Task<Result> GetPostsByCreatorAsync(string creatorId);
}
