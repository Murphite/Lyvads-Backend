using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Application.Implementions;

public class CreatorService : ICreatorService
{
    private readonly IRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IPaymentGatewayService _paymentGatewayService;

    public CreatorService(IRepository repository, IUnitOfWork unitOfWork, INotificationService notificationService, IPaymentGatewayService paymentGatewayService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _paymentGatewayService = paymentGatewayService;
    }

    public async Task<Result> CreatePostAsync(PostDto postDto, string creatorId)
    {
        var post = new Post
        {
            Id = Guid.NewGuid().ToString(),
            CreatorId = creatorId,
            Caption = postDto.Caption,
            MediaUrl = postDto.MediaUrl,
            Location = postDto.Location,
            Visibility = postDto.Visibility,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _repository.Add(post);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> CommentOnPostAsync(string postId, string userId, string content)
    {
        var comment = new Comment
        {
            Id = Guid.NewGuid().ToString(),
            PostId = postId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _repository.Add(comment);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> LikeCommentAsync(string commentId, string userId)
    {
        var like = new Like
        {
            Id = Guid.NewGuid().ToString(),
            CommentId = commentId,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _repository.Add(like);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> HandleRequestAsync(string requestId, RequestStatus status)
    {
        var request = await _repository.GetById<Request>(requestId);
        if (request == null)
            return new Error[] { new("Request.Error", "Request not found") };

        request.Status = status;
        _repository.Update(request);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> SendVideoToUserAsync(string requestId, string videoUrl)
    {
        var request = await _repository.GetById<Request>(requestId);
        if (request == null)
            return new Error[] { new("Request.Error", "Request not found") };

        // Send video logic...

        return Result.Success();
    }

    public async Task<Result> ViewWalletBalanceAsync(string creatorId)
    {
        var user = await _repository.GetById<ApplicationUser>(creatorId);
        if (user == null)
            return new Error[] { new("User.Error", "User not Found") };

        return Result.Success(new { user.WalletBalance });
    }

    public async Task<Result> WithdrawToBankAccountAsync(string creatorId, decimal amount, string currency)
    {
        var user = await _repository.GetById<ApplicationUser>(creatorId);
        if (user == null)
            return new Error[] { new("User.Error", "User not Found") };

        // Use Stripe or other payment gateway service to process the withdrawal
        var result = await _paymentGatewayService.Withdraw(user.StripeAccountId, amount, currency);
        if (!result.IsSuccess)
            return Result.Failure(new List<Error> { new Error("Payment.Error", "Withdrawal failed") });

        user.WalletBalance -= amount;
        _repository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> GetNotificationsAsync(string creatorId)
    {
        var notifications = await _repository.GetAll<Notification>()
                                             .Where(n => n.UserId == creatorId)
                                             .ToListAsync();

        return Result.Success(notifications);
    }

    public async Task<Result> GetPostsByCreatorAsync(string creatorId)
    {
        var posts = await _repository.GetAll<Post>()
                                     .Where(p => p.CreatorId == creatorId)
                                     .ToListAsync();

        return Result.Success(posts);
    }
}
