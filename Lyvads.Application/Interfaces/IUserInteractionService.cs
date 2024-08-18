
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;

namespace Lyvads.Application.Interfaces;

public interface IUserInteractionService
{
    Task<Result> AddCommentAsync(string userId, string content);
    Task<Result<CommentResponseDto>> EditCommentAsync(string commentId, string userId, string newContent);
    Task<Result> DeleteCommentAsync(string commentId, string userId);
    Task<Result> LikeContentAsync(string userId, string contentId);
    Task<Result> UnlikeContentAsync(string userId, string contentId);
    Task<Result> FundWalletAsync(string userId, decimal amount);
    Task<Result> CreateRequestAsync(CreateRequestDto createRequestDto);
}
