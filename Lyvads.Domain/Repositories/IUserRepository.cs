
using Lyvads.Domain.Entities;
using Lyvads.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Domain.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser> GetUserByIdAsync(string userId);
    Task AddCommentAsync(Comment comment);
    Task AddLikeAsync(Like like);
    Task<Like> GetLikeAsync(string userId, string contentId);
    Task RemoveLikeAsync(Like like);
    Task UpdateWalletBalanceAsync(string userId, decimal amount);
    Task<ApplicationUser?> GetUserWithCreatorAsync(string userId);
    Task AddFavoriteAsync(string userId, string creatorId);
    Task<Creator> GetCreatorByIdWithApplicationUser(string id);
    Task FollowCreatorAsync(string userId, string creatorId);
    Task<CreatorProfileDto> GetCreatorByIdAsync(string creatorId);
    Task<bool> CreatorExistsAsync(string creatorId);
    IQueryable<CreatorResponseDto> GetCreatorsWithMostEngagementAndFollowersAsync();
    Task<List<CreatorResponseDto>> GetFavoriteCreatorsAsync(string userId);
    Task<int> GetFollowerCountAsync(string creatorId);
    Task<int> GetLikesCountAsync(string postId);
    Task<List<string>> GetUserIdsWhoLikedPostAsync(string postId);
    Task UnfollowCreatorAsync(string userId, string creatorId);
    Task<Favorite?> GetFavoriteAsync(string userId, string creatorId);
    Task RemoveFavoriteAsync(string userId, string creatorId);
    Task<bool> IsUserFollowingCreatorAsync(string userId, string creatorId);
    Task<int> GetCreatorsFollowingCountAsync(string userId);
    Task<int> GetUsersFollowingCreatorCountAsync(string creatorId);
    Task<List<UserFollowerDto>> GetUsersFollowingCreatorDetailsAsync(string creatorId);
    Task<bool> IsCreatorInUserFavoritesAsync(string userId, string creatorId);

}
