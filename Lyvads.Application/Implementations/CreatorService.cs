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
using System.Reflection;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Repositories;
using Lyvads.Application.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;

namespace Lyvads.Application.Implementations;

public class CreatorService : ICreatorService
{
    private readonly IRepository _repository;
    private readonly ICreatorRepository _creatorRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly ILogger<CreatorService> _logger;
    private readonly IEmailService _emailService;
    private readonly IVerificationService _verificationService;
    private readonly IMediaService _mediaService;

    public CreatorService(IRepository repository,
        ICreatorRepository creatorRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IPaymentGatewayService paymentGatewayService,
        UserManager<ApplicationUser> userManager,
        ILogger<CreatorService> logger,
        IEmailService emailService,
        IVerificationService verificationService,
        IMediaService mediaService)
    {
        _repository = repository;
        _creatorRepository = creatorRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _paymentGatewayService = paymentGatewayService;
        _userManager = userManager;
        _logger = logger;
        _emailService = emailService;
        _verificationService = verificationService;
        _mediaService = mediaService;
    }

    public async Task<ServerResponse<PaginatorDto<IEnumerable<CreatorDto>>>> GetCreators(PaginationFilter paginationFilter)
    {
        _logger.LogInformation("******* Inside the GetCreators Method ********");

        if (paginationFilter == null)
        {
            return new ServerResponse<PaginatorDto<IEnumerable<CreatorDto>>>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Invalid pagination parameters",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Pagination filter cannot be null."
                }
            };
        }

        try
        {
            // Assuming you have a method to get all creators
            var creatorsQuery = _creatorRepository.GetCreators()
                .Include(c => c.ApplicationUser) // Include ApplicationUser data if needed
                .AsQueryable();

            // Apply pagination
            var paginatedResult = await Pagination.PaginateAsync(
                creatorsQuery.Select(c => new CreatorDto
                {
                    FullName = c.ApplicationUser.FullName,
                    Industry = c.ExclusiveDeals.Select(ed => ed.Industry).FirstOrDefault(),
                    AdvertAmount = c.AdvertAmount
                }),
                paginationFilter
            );

            if (paginatedResult.PageItems == null || !paginatedResult.PageItems.Any())
            {
                return new ServerResponse<PaginatorDto<IEnumerable<CreatorDto>>>
                {
                    IsSuccessful = false,
                    ResponseCode = "404",
                    ResponseMessage = "No creators found.",
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "404",
                        ResponseMessage = "No creators found matching the criteria."
                    }
                };
            }

            return new ServerResponse<PaginatorDto<IEnumerable<CreatorDto>>>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Creators retrieved successfully.",
                Data = paginatedResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving creators");
            return new ServerResponse<PaginatorDto<IEnumerable<CreatorDto>>>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "An error occurred while retrieving creators.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal server error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }

    public async Task<ServerResponse<PostResponseDto>> CreatePostAsync(PostDto postDto, string userId)
    {
        _logger.LogInformation("Creating post for creator with User ID: {UserId}", userId);

        // Check if the creator exists in the database by UserId
        var creator = await _repository.FindByCondition<Creator>(c => c.ApplicationUserId == userId);
        if (creator == null)
        {
            _logger.LogWarning("Creator with User ID: {UserId} not found.", userId);
            return new ServerResponse<PostResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Creator does not exist.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Creator not found."
                }
            };
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

        try
        {
            await _repository.Add(post);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Post created successfully for creator with User ID: {UserId}", userId);

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

            return new ServerResponse<PostResponseDto>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Post created successfully.",
                Data = postResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post for creator with User ID: {UserId}", userId);
            return new ServerResponse<PostResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "An error occurred while creating the post.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal server error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }

    public async Task<ServerResponse<PostResponseDto>> UpdatePostAsync(UpdatePostDto postDto, string userId)
    {
        _logger.LogInformation("Updating post with Post ID: {PostId} for User ID: {UserId}", postDto.PostId, userId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ServerResponse<PostResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found."
                }
            };
        }

        var post = _repository.GetAll<Post>().FirstOrDefault(x => x.Id == postDto.PostId);
        if (post == null)
        {
            _logger.LogWarning("Post with ID: {PostId} not found.", postDto.PostId);
            return new ServerResponse<PostResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Post not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Post not found."
                }
            };
        }

        var creator = await _repository.FindByCondition<Creator>(c => c.Id == post.CreatorId && c.ApplicationUserId == userId);
        if (creator == null)
        {
            _logger.LogWarning("Creator with User ID: {UserId} not authorized to update this post.", userId);
            return new ServerResponse<PostResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "Not authorized to update this post.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "403",
                    ResponseMessage = "Unauthorized."
                }
            };
        }

        post.Caption = postDto.Caption ?? post.Caption;
        post.Location = postDto.Location ?? post.Location;
        post.Visibility = postDto.Visibility ?? post.Visibility;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        _logger.LogInformation("Post object updated: {Post}", post);

        try
        {
            _repository.Update(post);
            await _unitOfWork.SaveChangesAsync();

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

            return new ServerResponse<PostResponseDto>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Post updated successfully.",
                Data = postResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post with Post ID: {PostId}", postDto.PostId);
            return new ServerResponse<PostResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "An error occurred while updating the post.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal server error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }

    public async Task<ServerResponse<object>> DeletePostAsync(string postId, string userId)
    {
        _logger.LogInformation("Deleting post with Post ID: {PostId} for User ID: {UserId}", postId, userId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found."
                }
            };
        }

        var post = _repository.GetAll<Post>().FirstOrDefault(x => x.Id == postId);
        if (post == null)
        {
            _logger.LogWarning("Post with ID: {PostId} not found.", postId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Post not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Post not found."
                }
            };
        }

        var creator = await _repository.FindByCondition<Creator>(c => c.Id == post.CreatorId && c.ApplicationUserId == userId);
        if (creator == null)
        {
            _logger.LogWarning("Creator with User ID: {UserId} not authorized to delete this post.", userId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "Not authorized to delete this post.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "403",
                    ResponseMessage = "Unauthorized."
                }
            };
        }

        try
        {
            _repository.Remove(post);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Post deleted successfully for User ID: {UserId}", userId);
            return new ServerResponse<object>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Post deleted successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post with Post ID: {PostId}", postId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "An error occurred while deleting the post.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Internal server error",
                    ResponseDescription = ex.Message
                }
            };
        }
    }

    public async Task<ServerResponse<CommentResponseDto>> CommentOnPostAsync(string postId, string userId, string content)
    {
        _logger.LogInformation("User {UserId} is commenting on post {PostId}", userId, postId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found.", userId);
            return new ServerResponse<CommentResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found."
                }

            };
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid().ToString(),
            PostId = postId,
            CommentBy = $"{user.FirstName} {user.LastName}",
            ApplicationUserId = userId,
            Content = content,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _repository.Add(comment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Comment added successfully to post {PostId} by user {UserId}", postId, userId);

        var commentResponse = new CommentResponseDto
        {
            CommentId = comment.Id,
            PostId = comment.PostId,
            UserId = comment.ApplicationUserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            CommentBy = comment.CommentBy,
        };

        return new ServerResponse<CommentResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Comment added to post successfully."
        };
    }

    public async Task<ServerResponse<LikeResponseDto>> LikeCommentAsync(string commentId, string userId)
    {
        _logger.LogInformation("User {UserId} is liking comment {CommentId}", userId, commentId);

        var comment = await _repository.FindByCondition<Comment>(c => c.Id == commentId);
        if (comment == null)
        {
            _logger.LogWarning("Comment {CommentId} not found.", commentId);
            return new ServerResponse<LikeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Comment does not exist..",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Comment does not exist.."
                }
            };
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found.", userId);
            return new ServerResponse<LikeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User does not exist.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User does not exist."
                }
            };
        }

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

        _logger.LogInformation("Comment {CommentId} liked by user {UserId}", commentId, userId);

        var likeResponse = new LikeResponseDto
        {
            LikeId = like.Id,
            CommentId = like.CommentId,
            UserId = like.UserId,
            LikedBy = like.LikedBy,
            CreatedAt = like.CreatedAt
        };

        return new ServerResponse<LikeResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Comment liked successfully."
        };
    }

    public async Task<ServerResponse<LikeResponseDto>> LikePostAsync(string postId, string userId)
    {
        _logger.LogInformation("User with ID: {UserId} is liking post with ID: {PostId}", userId, postId);


        // Check if the post exists
        var post = await _repository.FindByCondition<Post>(p => p.Id == postId);
        if (post == null)
        {
            _logger.LogWarning("Post with ID: {PostId} not found.", postId);
            return new ServerResponse<LikeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Post not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Post not found."
                }
            };
        }

        // Check if the user exists
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", userId);
            return new ServerResponse<LikeResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found."
                }
            };
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

        return new ServerResponse<LikeResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Post liked successfully."
        };
    }

    public async Task<ServerResponse<RequestResponseDto>> HandleRequestAsync(string requestId, RequestStatus status)
    {
        _logger.LogInformation("Handling request with ID: {RequestId}, setting status to {Status}", requestId, status);

        // Retrieve the request entity
        var request = await _repository.GetById<Request>(requestId);
        if (request == null)
        {
            _logger.LogWarning("Request with ID: {RequestId} not found.", requestId);
            return new ServerResponse<RequestResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Request not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Request not found."
                }
            };
        }

        if (string.IsNullOrEmpty(request.UserId))
        {
            _logger.LogWarning("User ID for request ID: {RequestId} is null or empty.", requestId);
            return new ServerResponse<RequestResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User ID is missing.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User ID is missing."
                }
            };
        }

        // Check if the user exists
        var user = await _repository.GetById<ApplicationUser>(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found for request ID: {RequestId}", request.UserId, requestId);
            return new ServerResponse<RequestResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found."
                }
            };
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

        return new ServerResponse<RequestResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Request handled successfully."
        };
    }

    public async Task<ServerResponse<VideoResponseDto>> SendVideoToUserAsync(string requestId, IFormFile video)
    {
        _logger.LogInformation("Sending video for request with ID: {RequestId}", requestId);

        // Check if the request exists
        var request = await _repository.GetById<Request>(requestId);
        if (request == null)
        {
            _logger.LogWarning("Request with ID: {RequestId} not found.", requestId);
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Request not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Request not found."
                }
            };
        }

        // Check if the user ID is not null or empty
        if (string.IsNullOrEmpty(request.UserId))
        {
            _logger.LogWarning("User ID for request with ID: {RequestId} is null or empty.", requestId);
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User ID is missing.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User ID is missing."
                }
            };
        }

        // Check if the user exists
        var user = await _repository.GetById<ApplicationUser>(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", request.UserId);
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found."
                }
            };
        }

        // Upload the video to Cloudinary
        var uploadResult = await _mediaService.UploadVideoAsync(video, "videos");

        if (uploadResult["Code"] != "200")
        {
            _logger.LogError("Failed to upload video for request with ID: {RequestId}", requestId);
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "Failed to upload video.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Failed to upload video."
                }
            };
        }

        var videoUrl = uploadResult["Url"];

        _logger.LogInformation("Video sent successfully for request with ID: {RequestId}", requestId);

        var videoResponse = new VideoResponseDto
        {
            RequestId = request.Id,
            VideoUrl = videoUrl,
            UserId = user.Id,
            UserName = user.FullName
        };

        return new ServerResponse<VideoResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Video sent successfully.",
            Data = videoResponse
        };
    }

    public async Task<ServerResponse<WalletBalanceDto>> ViewWalletBalanceAsync(string creatorId)
    {
        _logger.LogInformation("Viewing wallet balance for creator with ID: {CreatorId}", creatorId);

        // Check if the creator exists
        var creator = await _repository.GetById<Creator>(creatorId);
        if (creator == null)
        {
            _logger.LogWarning("Creator with ID: {CreatorId} not found.", creatorId);
            return new ServerResponse<WalletBalanceDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Creator not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Creator not found."
                }
            };

        }

        // Check if the user ID is not null or empty
        if (string.IsNullOrEmpty(creator.ApplicationUserId))
        {
            _logger.LogWarning("User ID for creator with ID: {CreatorId} is null or empty.", creatorId);
            //return new Error[] { new("User.Error", "User ID is missing.") };
            return new ServerResponse<WalletBalanceDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User ID is missing.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User ID is missing."
                }
            };
        }

        // Check if the associated user exists
        var user = await _repository.GetById<ApplicationUser>(creator.ApplicationUserId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found.", creator.ApplicationUserId);
            return new ServerResponse<WalletBalanceDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found."
                }
            };
        }

        var walletBalanceResponse = new WalletBalanceDto
        {
            CreatorId = creatorId,
            WalletBalance = user.WalletBalance,
            UserName = user.FullName
        };

        _logger.LogInformation("Wallet balance viewed for creator with ID: {CreatorId}", creatorId);

        return new ServerResponse<WalletBalanceDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Wallet balance successfully fetched."
        };
    }

    public async Task<ServerResponse<WithdrawResponseDto>> WithdrawToBankAccountAsync(string creatorId, decimal amount, string currency)
    {
        _logger.LogInformation("Initiating withdrawal for creator with ID: {CreatorId}, amount: {Amount}, currency: {Currency}", creatorId, amount, currency);

        // Retrieve the Creator entity
        var creator = await _repository.GetById<Creator>(creatorId);
        if (creator == null)
        {
            _logger.LogWarning("Creator with ID: {CreatorId} not found.", creatorId);
            return new ServerResponse<WithdrawResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Creator not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Creator not found."
                }
            };
        }

        // Use the navigation property to get the associated ApplicationUser
        var user = creator.ApplicationUser;
        if (user == null)
        {
            _logger.LogWarning("ApplicationUser for Creator with ID: {CreatorId} not found.", creatorId);
            //return new Error[] { new("User.Error", "Associated User not Found") };
            return new ServerResponse<WithdrawResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Associated user not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Associated user not found."
                }
            };
        }

        if (string.IsNullOrEmpty(user.StripeAccountId))
        {
            _logger.LogWarning("StripeAccountId for User with ID: {UserId} is null or empty.", user.Id);
            return new ServerResponse<WithdrawResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User's Stripe Account ID is not set.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User's Stripe Account ID is not set."
                }
            };
        }


        // Perform the withdrawal using the payment gateway service
        var result = await _paymentGatewayService.Withdraw(user.StripeAccountId, amount, currency);
        if (!result.IsSuccess)
        {
            _logger.LogError("Withdrawal failed for creator with ID: {CreatorId}, amount: {Amount}," +
                " currency: {Currency}", creatorId, amount, currency);
            return new ServerResponse<WithdrawResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Withdrawal failed.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Withdrawal failed."
                }
            };
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

        return new ServerResponse<WithdrawResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Withdrawal successful."
        };
    }

    public async Task<ServerResponse<IEnumerable<NotificationResponseDto>>> GetNotificationsAsync(string creatorId)
    {
        _logger.LogInformation("Getting notifications for creator with ID: {CreatorId}", creatorId);

        // Check if the user exists
        var user = await _repository.GetById<ApplicationUser>(creatorId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {CreatorId} not found.", creatorId);
            return new ServerResponse<IEnumerable<NotificationResponseDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found."
                }
            };
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
        }).ToList();

        _logger.LogInformation("Notifications retrieved for creator with ID: {CreatorId}", creatorId);
        return new ServerResponse<IEnumerable<NotificationResponseDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Notifications retrieved successfully.",
            Data = notificationResponses
        };
    }

    public async Task<ServerResponse<IEnumerable<PostResponseDto>>> GetPostsByCreatorAsync(string creatorId)
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
        }).ToList();

        _logger.LogInformation("Posts retrieved for creator with ID: {CreatorId}", creatorId);

        return new ServerResponse<IEnumerable<PostResponseDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Posts retrieved successfully.",
            Data = postResponses
        };
    }

    public async Task<ServerResponse<CreatorProfileResponseDto>> UpdateCreatorSetUpRatesAsync(UpdateCreatorProfileDto dto, string userId)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            //return new Error[] { new("User.Error", "User Not Found") };
            return new ServerResponse<CreatorProfileResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found."
                }
            };

        // Find the creator associated with the user
        var creator = _repository.GetAll<Creator>()
            .FirstOrDefault(x => x.ApplicationUserId == user.Id);

        if (creator == null)
            //return new Error[] { new("Creator.Error", "Creator Not Found") };
            return new ServerResponse<CreatorProfileResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Creator not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Creator not found."
                }
            };

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

        return new ServerResponse<CreatorProfileResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Creator's profile updated successfully.",
            
        };
       
    }

    public async Task<ServerResponse<List<FilterCreatorDto>>> SearchCreatorsAsync(decimal? minPrice, decimal? maxPrice, string location, string industry)
    {
        _logger.LogInformation("Searching for creators with filters - MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, Location: {Location}, Industry: {Industry}",
                                minPrice, maxPrice, location, industry);

        var query = _creatorRepository.GetCreators();

        if (minPrice.HasValue)
        {
            query = query.Where(c => c.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(c => c.Price <= maxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            // Use Contains for keyword-based matching
            query = query.Where(c => c.ApplicationUser.Location.Contains(location));
        }

        if (!string.IsNullOrWhiteSpace(industry))
        {
            // Use Contains for keyword-based matching
            query = query.Where(c => c.ApplicationUser.Occupation.Contains(industry));
        }

        var creators = await query.ToListAsync();

        // Check if no creators were found
        if (creators == null || !creators.Any())
        {
            _logger.LogInformation("No creators found matching the filters.");
            return new ServerResponse<List<FilterCreatorDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "No creators found matching the search criteria.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "No creators found."
                }
            };
        }

        var creatorDtos = creators.Select(c => new FilterCreatorDto
        {
            CreatorId = c.Id,
            FullName = $"{c.ApplicationUser.FirstName} {c.ApplicationUser.LastName}",
            Price = c.Price,
            Location = c.ApplicationUser.Location,
            Industry = c.ApplicationUser.Occupation
        }).ToList();

        _logger.LogInformation("Found {CreatorCount} creators matching the filters", creatorDtos.Count);

        // Return the list of creator DTOs as a successful response
        return new ServerResponse<List<FilterCreatorDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Creators found successfully.",
            Data = creatorDtos // Include the list of creator DTOs
        };
    }









    private string GenerateVerificationCode()
    {
        // Generate a random 5-digit number as verification code
        var random = new Random();
        return random.Next(10000, 99999).ToString();
    }
}
