using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos;
using Lyvads.Domain.Enums;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Http;


namespace Lyvads.Application.Interfaces;

public interface ICreatorService
{
    Task<ServerResponse<PaginatorDto<IEnumerable<CreatorDto>>>> GetCreators(PaginationFilter paginationFilter);
    Task<ServerResponse<PostResponseDto>> UpdatePostAsync(string postId, UpdatePostDto postDto, PostVisibility visibility, string userId, IFormFile photo);
    Task<ServerResponse<PostResponseDto>> CreatePostAsync(PostDto postDto, PostVisibility visibility, string userId, IFormFile photo);
    Task<ServerResponse<CreatorProfileResponseDto>> UpdateCreatorSetUpRatesAsync(UpdateCreatorProfileDto dto, string userId);
    //Task<ServerResponse<PostResponseDto>> CreatePostAsync(PostDto postDto, string userId);
    Task<ServerResponse<object>> DeletePostAsync(string postId, string userId);
    //Task<ServerResponse<PostResponseDto>> UpdatePostAsync(UpdatePostDto postDto, string userId);
    Task<ServerResponse<CommentResponseDto>> CommentOnPostAsync(string postId, string userId, string content);
    Task<ServerResponse<LikeResponseDto>> LikeCommentAsync(string commentId, string userId);
    Task<ServerResponse<LikeResponseDto>> LikePostAsync(string postId, string userId);
    Task<ServerResponse<RequestResponseDto>> HandleRequestAsync(string requestId, RequestStatus status);
    Task<ServerResponse<VideoResponseDto>> SendVideoToUserAsync(string requestId, IFormFile video);
    Task<ServerResponse<WalletBalanceDto>> ViewWalletBalanceAsync(string creatorId);
    Task<ServerResponse<WithdrawResponseDto>> WithdrawToBankAccountAsync(string creatorId, decimal amount, string currency);
    Task<ServerResponse<IEnumerable<NotificationResponseDto>>> GetNotificationsAsync(string creatorId);
    Task<ServerResponse<IEnumerable<PostResponseDto>>> GetPostsByCreatorAsync(string creatorId);
    Task<ServerResponse<List<FilterCreatorDto>>> SearchCreatorsAsync(decimal? minPrice, decimal? maxPrice, string location, string industry);
}
