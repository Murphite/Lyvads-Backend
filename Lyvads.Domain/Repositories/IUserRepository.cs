
using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser> GetUserByIdAsync(string userId);
    Task AddCommentAsync(Comment comment);
    Task AddLikeAsync(Like like);
    Task<Like> GetLikeAsync(string userId, string contentId);
    Task RemoveLikeAsync(Like like);
    Task UpdateWalletBalanceAsync(string userId, decimal amount);
}
