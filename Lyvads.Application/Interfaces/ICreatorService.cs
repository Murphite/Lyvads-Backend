using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos;
using Lyvads.Domain.Enums;
using Lyvads.Application.Dtos.RegularUserDtos;


namespace Lyvads.Application.Interfaces;

public interface ICreatorService
{
    Task<Result<CreatorProfileResponseDto>> UpdateCreatorSetUpRatesAsync(UpdateCreatorProfileDto dto, string userId);
    Task<Result<PostResponseDto>> CreatePostAsync(PostDto postDto, string userId);
    Task<Result> DeletePostAsync(int postId, string userId);
    Task<Result<PostResponseDto>> UpdatePostAsync(UpdatePostDto postDto, string userId);
    Task<Result<CommentResponseDto>> CommentOnPostAsync(string postId, string userId, string content);
    Task<Result<LikeResponseDto>> LikeCommentAsync(string commentId, string userId);
    Task<Result<LikeResponseDto>> LikePostAsync(string postId, string userId);
    Task<Result<RequestResponseDto>> HandleRequestAsync(string requestId, RequestStatus status);
    Task<Result<VideoResponseDto>> SendVideoToUserAsync(string requestId, string videoUrl);
    Task<Result<WalletBalanceDto>> ViewWalletBalanceAsync(string creatorId);
    Task<Result<WithdrawResponseDto>> WithdrawToBankAccountAsync(string creatorId, decimal amount, string currency);
    Task<Result<IEnumerable<NotificationResponseDto>>> GetNotificationsAsync(string creatorId);
    Task<Result<IEnumerable<PostResponseDto>>> GetPostsByCreatorAsync(string creatorId);
}
