using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Lyvads.Application.Dtos;
using Lyvads.Infrastructure.Repositories;

namespace Lyvads.Application.Implementions;

public class UserInteractionService : IUserInteractionService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository _repository;
    private readonly ILogger<UserInteractionService> _logger;

    public UserInteractionService(IUserRepository userRepository, IRepository repository, ILogger<UserInteractionService> logger)
    {
        _userRepository = userRepository;
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> AddCommentAsync(string userId, string content)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return new Error[] { new("Comment.Error", "User not found") };
        

        var comment = new Comment
        {
            UserId = userId,
            Content = content,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddCommentAsync(comment);
        return Result.Success();
    }

    public async Task<Result> LikeContentAsync(string userId, string contentId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return new Error[] { new("Like.Error", "User not found") };
      
        var like = new Like
        {
            UserId = userId,
            ContentId = contentId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddLikeAsync(like);
        return Result.Success();
    }

    public async Task<Result> FundWalletAsync(string userId, decimal amount)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return new Error[] { new("Wallet.Error", "User not found") };

        await _userRepository.UpdateWalletBalanceAsync(userId, amount);
        return Result.Success();
    }
}
