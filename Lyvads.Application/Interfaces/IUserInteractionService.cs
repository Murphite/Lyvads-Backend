
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Interfaces;

public interface IUserInteractionService
{
    Task<ServerResponse<string>> MakeRequestAsync(CreateRequestDto createRequestDto);
    Task<ServerResponse<CommentResponseDto>> EditCommentAsync(string commentId, string userId, string newContent);
    Task<ServerResponse<object>> AddCommentAsync(string userId, string content);
    Task<ServerResponse<object>> DeleteCommentAsync(string userId, string commentId);
    Task<ServerResponse<object>> LikeContentAsync(string userId, string contentId);
    Task<ServerResponse<object>> UnlikeContentAsync(string userId, string contentId);
    Task<ServerResponse<object>> FundWalletAsync(string userId, decimal amount);
    

    //Task<ServerResponse<object>> CreateRequestAsync(CreateRequestDto createRequestDto);
}
