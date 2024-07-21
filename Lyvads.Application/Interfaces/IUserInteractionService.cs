
using Lyvads.Application.Dtos;

namespace Lyvads.Application.Interfaces;

public interface IUserInteractionService
{
    Task<Result> AddCommentAsync(string userId, string content);
    Task<Result> LikeContentAsync(string userId, string contentId);
    Task<Result> FundWalletAsync(string userId, decimal amount);
}
