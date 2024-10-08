
using Lyvads.Domain.Entities;
using Lyvads.Shared.DTOs;

namespace Lyvads.Domain.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser> GetUserByIdAsync(string userId);
    Task AddCommentAsync(Comment comment);
    Task AddLikeAsync(Like like);
    Task<Like> GetLikeAsync(string userId, string contentId);
    Task RemoveLikeAsync(Like like);
    Task UpdateWalletBalanceAsync(string userId, decimal amount);

    Task AddFavoriteAsync(string userId, string creatorId);
    Task FollowCreatorAsync(string userId, string creatorId);
    Task<CreatorProfileDto> GetCreatorByIdAsync(string creatorId);
    //Task<Creator?> GetCreatorByIdAsync(string creatorId);
    Task<List<CreatorResponseDto>> GetCreatorsWithMostEngagementAndFollowersAsync(int count);
    Task<List<CreatorResponseDto>> GetFavoriteCreatorsAsync(string userId);
    Task<int> GetFollowerCountAsync(string creatorId);
    Task<int> GetLikesCountAsync(string postId);
    Task<List<string>> GetUserIdsWhoLikedPostAsync(string postId);
    Task UnfollowCreatorAsync(string userId, string creatorId);
}
