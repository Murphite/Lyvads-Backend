using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Application.Dtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Domain.Interfaces;
using static Lyvads.Application.Implementions.AuthService;
using System.Reflection;

namespace Lyvads.Application.Implementions;

public class CreatorService : ICreatorService
{
    private readonly IRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly ILogger<CreatorService> _logger;
    private readonly IEmailService _emailService;
    private readonly IVerificationService _verificationService;

    public CreatorService(IRepository repository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IPaymentGatewayService paymentGatewayService,
        UserManager<ApplicationUser> userManager,
        ILogger<CreatorService> logger,
        IEmailService emailService,
        IVerificationService verificationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _paymentGatewayService = paymentGatewayService;
        _userManager = userManager;
        _logger = logger;
        _emailService = emailService;
        _verificationService = verificationService;
    }

    
    public async Task<Result<PostResponseDto>> CreatePostAsync(PostDto postDto, string userId)
    {
        _logger.LogInformation("Creating post for creator with User ID: {UserId}", userId);

        // Check if the creator exists in the database by UserId
        var creator = await _repository.FindByCondition<Creator>(c => c.UserId == userId);
        if (creator == null)
        {
            _logger.LogWarning("Creator with User ID: {UserId} not found.", userId);
            return new Error[] { new("Creator.Error", "Creator does not exist.") };
        }

        // Create the new post
        var post = new Post
        {
            CreatorId = creator.Id,
            Caption = postDto.Caption,
            MediaUrl = postDto.MediaUrl,
            Location = postDto.Location,
            Visibility = postDto.Visibility,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _logger.LogInformation("Post object created: {Post}", post);

        // Add the post to the repository and save changes
        await _repository.Add(post);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Post created successfully for creator with User ID: {UserId}", userId);

        // Prepare the response DTO
        var postResponse = new PostResponseDto
        {
            PostId = post.Id,
            CreatorId = creator.Id,
            CreatorName = creator.ApplicationUser?.FullName,
            Caption = post.Caption,
            MediaUrl = post.MediaUrl,
            Location = post.Location,
            Visibility = post.Visibility.ToString(),
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };

        return Result<PostResponseDto>.Success(postResponse);
    }

    public async Task<Result<PostResponseDto>> UpdatePostAsync(UpdatePostDto postDto, string userId)
    {
        _logger.LogInformation("Updating post with Post ID: {PostId} for User ID: {UserId}", postDto.PostId, userId);

        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new Error[] { new("User.Error", "User Not Found") };

        // Fetch the post by PostId
        var post = _repository.GetAll<Post>()
            .FirstOrDefault(x => x.Id == user.Id);
        if (post == null)
        {
            _logger.LogWarning("Post with ID: {PostId} not found.", postDto.PostId);
            return new Error[] { new("Post.Error", "Post not found.") };
        }

        // Check if the post belongs to the creator (validate userId)
        var creator = await _repository.FindByCondition<Creator>(c => c.Id == post.CreatorId && c.UserId == userId);
        if (creator == null)
        {
            _logger.LogWarning("Creator with User ID: {UserId} not authorized to update this post.", userId);
            return new Error[] { new("Authorization.Error", "You are not authorized to update this post.") };
        }

        // Update the allowed fields (except MediaUrl)
        post.Caption = postDto.Caption ?? post.Caption;
        post.Location = postDto.Location ?? post.Location;
        post.Visibility = postDto.Visibility ?? post.Visibility;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        _logger.LogInformation("Post object updated: {Post}", post);

        // Save the changes to the database
        _repository.Update(post);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Post updated successfully for User ID: {UserId}", userId);

        // Prepare the response DTO
        var postResponse = new PostResponseDto
        {
            PostId = post.Id,
            CreatorId = creator.Id,
            CreatorName = creator.ApplicationUser?.FullName,
            Caption = post.Caption,
            MediaUrl = post.MediaUrl, // MediaUrl remains unchanged
            Location = post.Location,
            Visibility = post.Visibility.ToString(),
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };

        return Result<PostResponseDto>.Success(postResponse);
    }

    public async Task<Result> DeletePostAsync(int postId, string userId)
    {
        _logger.LogInformation("Deleting post with Post ID: {PostId} for User ID: {UserId}", postId, userId);

        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new Error[] { new("User.Error", "User Not Found") };

        // Fetch the post by PostId
        var post = _repository.GetAll<Post>()
            .FirstOrDefault(x => x.Id == user.Id);
        if (post == null)
        {
            _logger.LogWarning("Post with ID: {PostId} not found.", post);
            return new Error[] { new("Post.Error", "Post not found.") };
        }

        // Check if the post belongs to the creator (validate userId)
        var creator = await _repository.FindByCondition<Creator>(c => c.Id == post.CreatorId && c.UserId == userId);
        if (creator == null)
        {
            _logger.LogWarning("Creator with User ID: {UserId} not authorized to delete this post.", userId);
            return new Error[] { new("Authorization.Error", "You are not authorized to delete this post.") };
        }

        // Delete the post
        _repository.Remove(post);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Post deleted successfully for User ID: {UserId}", userId);

        return Result.Success();
    }

    public async Task<Result<CommentResponseDto>> CommentOnPostAsync(string postId, string userId, string content)
    {
        _logger.LogInformation("User with ID: {UserId} is commenting on post with ID: {PostId}", userId, postId);

        // Check if the user exists in the database
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", userId);
            return new Error[] { new("User.Error", "User not found.") };
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid().ToString(),
            PostId = postId,
            CommentBy = $"{user.FirstName} {user.LastName}",
            UserId = userId,
            Content = content,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _repository.Add(comment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Comment added successfully to post with ID: {PostId} by user with ID: {UserId}", postId, userId);

        var commentResponse = new CommentResponseDto
        {
            CommentId = comment.Id,
            PostId = comment.PostId,
            UserId = comment.UserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            CommentBy = comment.CommentBy,
        };

        return Result<CommentResponseDto>.Success(commentResponse);
    }

    public async Task<Result<LikeResponseDto>> LikeCommentAsync(string commentId, string userId)
    {
        _logger.LogInformation("User with ID: {UserId} is liking comment with ID: {CommentId}", userId, commentId);

        // Check if the comment exists
        var comment = await _repository.FindByCondition<Comment>(c => c.Id == commentId);
        if (comment == null)
        {
            _logger.LogWarning("Comment with ID: {CommentId} not found.", commentId);
            return new Error[] { new("Comment.Error", "Comment does not exist.") };
        }

        // Check if the user exists
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", userId);
            return new Error[] { new("User.Error", "User does not exist.") };
        }

        // Proceed with liking the comment
        var like = new Like
        {
            Id = Guid.NewGuid().ToString(),
            CommentId = commentId,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            LikedBy = $"{user.FirstName} {user.LastName}",
        };

        await _repository.Add(like);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Comment with ID: {CommentId} liked by user with ID: {UserId}", commentId, userId);

        // Prepare the response DTO
        var likeResponse = new LikeResponseDto
        {
            LikeId = like.Id,
            CommentId = like.CommentId,
            UserId = like.UserId,
            LikedBy = $"{user.FirstName} {user.LastName}",
            CreatedAt = like.CreatedAt
        };

        return Result<LikeResponseDto>.Success(likeResponse);
    }

    public async Task<Result<LikeResponseDto>> LikePostAsync(string postId, string userId)
    {
        _logger.LogInformation("User with ID: {UserId} is liking post with ID: {PostId}", userId, postId);


        // Check if the post exists
        var post = await _repository.FindByCondition<Post>(p => p.Id == postId);
        if (post == null)
        {
            _logger.LogWarning("Post with ID: {PostId} not found.", postId);
            return new Error[] { new("Post.Error", "Post not found.") };
        }

        // Check if the user exists
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", userId);
            return new Error[] { new("User.Error", "User not found.") };
        }

        // Create the Like entity
        var like = new Like
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            PostId = postId,
            CreatedAt = DateTimeOffset.UtcNow,
            LikedBy = $"{user.FirstName} {user.LastName}",
        };

        // Add the Like to the repository
        await _repository.Add(like);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Post with ID: {PostId} liked successfully by user with ID: {UserId}.", postId, userId);

        // Prepare the response DTO
        var likeResponse = new LikeResponseDto
        {
            LikeId = like.Id,
            PostId = like.PostId,
            UserId = like.UserId,
            CreatedAt = like.CreatedAt,
            LikedBy = $"{user.FirstName} {user.LastName}",
        };

        return Result<LikeResponseDto>.Success(likeResponse);
    }

    public async Task<Result<RequestResponseDto>> HandleRequestAsync(string requestId, RequestStatus status)
    {
        _logger.LogInformation("Handling request with ID: {RequestId}, setting status to {Status}", requestId, status);

        // Retrieve the request entity
        var request = await _repository.GetById<Request>(requestId);
        if (request == null)
        {
            _logger.LogWarning("Request with ID: {RequestId} not found.", requestId);
            return Result.Failure<RequestResponseDto>(new List<Error> { new Error("Request.Error", "Request not found") });
        }

        if (string.IsNullOrEmpty(request.UserId))
        {
            _logger.LogWarning("User ID for request ID: {RequestId} is null or empty.", requestId);
            return Result.Failure<RequestResponseDto>(new List<Error> { new Error("User.Error", "User ID is missing") });
        }

        // Check if the user exists
        var user = await _repository.GetById<ApplicationUser>(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found for request ID: {RequestId}", request.UserId, requestId);
            return Result.Failure<RequestResponseDto>(new List<Error> { new Error("User.Error", "User not found") });
        }

        // Update the request status
        request.Status = status;
        _repository.Update(request);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Request with ID: {RequestId} handled successfully", requestId);

        // Prepare the response DTO
        var requestResponse = new RequestResponseDto
        {
            RequestId = request.Id,
            Status = request.Status.ToString(),
            UserId = user.Id,
            FullName = user.FullName
        };

        return Result<RequestResponseDto>.Success(requestResponse);
    }

    public async Task<Result<VideoResponseDto>> SendVideoToUserAsync(string requestId, string videoUrl)
    {
        _logger.LogInformation("Sending video for request with ID: {RequestId}", requestId);

        // Check if the request exists
        var request = await _repository.GetById<Request>(requestId);
        if (request == null)
        {
            _logger.LogWarning("Request with ID: {RequestId} not found.", requestId);
            return new Error[] { new("Post.Error", "Request not found.") };
        }

        // Check if the user ID is not null or empty
        if (string.IsNullOrEmpty(request.UserId))
        {
            _logger.LogWarning("User ID for request with ID: {RequestId} is null or empty.", requestId);
            return new Error[] { new("User.Error", "User ID is missing.") };
        }

        // Check if the user exists
        var user = await _repository.GetById<ApplicationUser>(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", request.UserId);
            return new Error[] { new("User.Error", "User not found.") };
        }

        // Send video logic...
        // Add your logic to send the video to the user based on the request.

        _logger.LogInformation("Video sent successfully for request with ID: {RequestId}", requestId);

        var videoResponse = new VideoResponseDto
        {
            RequestId = request.Id,
            VideoUrl = videoUrl,
            UserId = user.Id,
            UserName = user.FullName
        };

        return Result<VideoResponseDto>.Success(videoResponse);
    }

    public async Task<Result<WalletBalanceDto>> ViewWalletBalanceAsync(string creatorId)
    {
        _logger.LogInformation("Viewing wallet balance for creator with ID: {CreatorId}", creatorId);

        // Check if the creator exists
        var creator = await _repository.GetById<Creator>(creatorId);
        if (creator == null)
        {
            _logger.LogWarning("Creator with ID: {CreatorId} not found.", creatorId);
            return new Error[] { new("Creator.Error", "Creator not found.") };

        }

        // Check if the user ID is not null or empty
        if (string.IsNullOrEmpty(creator.UserId))
        {
            _logger.LogWarning("User ID for creator with ID: {CreatorId} is null or empty.", creatorId);
            return new Error[] { new("User.Error", "User ID is missing.") };
        }

        // Check if the associated user exists
        var user = await _repository.GetById<ApplicationUser>(creator.UserId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", creator.UserId);
            return new Error[] { new("User.Error", "User not found.") };
        }

        var walletBalanceResponse = new WalletBalanceDto
        {
            CreatorId = creatorId,
            WalletBalance = user.WalletBalance,
            UserName = user.FullName
        };

        _logger.LogInformation("Wallet balance viewed for creator with ID: {CreatorId}", creatorId);

        return Result<WalletBalanceDto>.Success(walletBalanceResponse);
    }

    public async Task<Result<WithdrawResponseDto>> WithdrawToBankAccountAsync(string creatorId, decimal amount, string currency)
    {
        _logger.LogInformation("Initiating withdrawal for creator with ID: {CreatorId}, amount: {Amount}, currency: {Currency}", creatorId, amount, currency);

        // Retrieve the Creator entity
        var creator = await _repository.GetById<Creator>(creatorId);
        if (creator == null)
        {
            _logger.LogWarning("Creator with ID: {CreatorId} not found.", creatorId);
            return new Error[] { new("Creator.Error", "Creator not Found") };
        }

        // Use the navigation property to get the associated ApplicationUser
        var user = creator.ApplicationUser;
        if (user == null)
        {
            _logger.LogWarning("ApplicationUser for Creator with ID: {CreatorId} not found.", creatorId);
            return new Error[] { new("User.Error", "Associated User not Found") };
        }

        if (string.IsNullOrEmpty(user.StripeAccountId))
        {
            _logger.LogWarning("StripeAccountId for User with ID: {UserId} is null or empty.", user.Id);
            return new Error[] { new("Payment.Error", "User's Stripe Account ID is not set") };
        }


        // Perform the withdrawal using the payment gateway service
        var result = await _paymentGatewayService.Withdraw(user.StripeAccountId, amount, currency);
        if (!result.IsSuccess)
        {
            _logger.LogError("Withdrawal failed for creator with ID: {CreatorId}, amount: {Amount}, currency: {Currency}", creatorId, amount, currency);
            return new Error[] { new("Payment.Error", "Withdrawal failed") };
        }

        // Update the user's wallet balance
        user.WalletBalance -= amount;
        _repository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Withdrawal successful for creator with ID: {CreatorId}, amount: {Amount}, currency: {Currency}", creatorId, amount, currency);

        // Prepare the response DTO
        var withdrawResponse = new WithdrawResponseDto
        {
            CreatorId = creatorId,
            Amount = amount,
            Currency = currency,
            RemainingBalance = user.WalletBalance,
            TransactionDate = DateTimeOffset.UtcNow
        };

        return Result<WithdrawResponseDto>.Success(withdrawResponse);
    }

    public async Task<Result<IEnumerable<NotificationResponseDto>>> GetNotificationsAsync(string creatorId)
    {
        _logger.LogInformation("Getting notifications for creator with ID: {CreatorId}", creatorId);

        // Check if the user exists
        var user = await _repository.GetById<ApplicationUser>(creatorId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {CreatorId} not found.", creatorId);
            return new Error[] { new("User.Error", "User not found.") };
        }

        // Retrieve notifications for the creator
        var notifications = await _repository.GetAll<Notification>()
                                             .Where(n => n.UserId == creatorId)
                                             .ToListAsync();

        // Map notifications to NotificationResponseDto
        var notificationResponses = notifications.Select(n => new NotificationResponseDto
        {
            NotificationId = n.Id,
            Content = n.Content,
            CreatedAt = n.CreatedAt
        });

        _logger.LogInformation("Notifications retrieved for creator with ID: {CreatorId}", creatorId);

        return Result<IEnumerable<NotificationResponseDto>>.Success(notificationResponses);
    }

    public async Task<Result<IEnumerable<PostResponseDto>>> GetPostsByCreatorAsync(string creatorId)
    {
        _logger.LogInformation("Getting posts for creator with ID: {CreatorId}", creatorId);

        var posts = await _repository.GetAll<Post>()
                                     .Where(p => p.CreatorId == creatorId)
                                     .ToListAsync();

        var postResponses = posts.Select(p => new PostResponseDto
        {
            PostId = p.Id,
            CreatorId = p.CreatorId,
            CreatorName = p.Creator?.ApplicationUser?.FullName ?? "Unknown",
            Caption = p.Caption,
            MediaUrl = p.MediaUrl,
            Location = p.Location,
            Visibility = p.Visibility.ToString(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });

        _logger.LogInformation("Posts retrieved for creator with ID: {CreatorId}", creatorId);

        return Result<IEnumerable<PostResponseDto>>.Success(postResponses);
    }

    

    public async Task<Result<CreatorProfileResponseDto>> UpdateCreatorSetUpRatesAsync(UpdateCreatorProfileDto dto, string userId)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return new Error[] { new("User.Error", "User Not Found") };

        // Find the creator associated with the user
        var creator = _repository.GetAll<Creator>()
            .FirstOrDefault(x => x.UserId == user.Id);

        if (creator == null)
            return new Error[] { new("Creator.Error", "Creator Not Found") };

        // Update the creator's details
        creator.SimpleAdvert = dto.SimpleAdvert ?? creator.SimpleAdvert;
        creator.WearBrand = dto.WearBrand ?? creator.WearBrand;
        creator.SongAdvert = dto.SongAdvert ?? creator.SongAdvert;
        creator.Request = dto.Request ?? creator.Request;        

        // Save the updates to the database
        _repository.Update(creator);
        await _userManager.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Prepare the response DTO
        var creatorProfileResponse = new CreatorProfileResponseDto
        {           
            SimpleAdvert = creator.SimpleAdvert,
            WearBrand = creator.WearBrand,
            SongAdvert = creator.SongAdvert,
            Request = creator.Request,
        };

        return Result<CreatorProfileResponseDto>.Success(creatorProfileResponse);
    }




    private string GenerateVerificationCode()
    {
        // Generate a random 5-digit number as verification code
        var random = new Random();
        return random.Next(10000, 99999).ToString();
    }
}
