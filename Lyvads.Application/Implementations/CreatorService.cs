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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lyvads.Application.Implementations;

public class CreatorService : ICreatorService
{
    private readonly IRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IRegularUserRepository _regularUserRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPostRepository _postRepository;
    private readonly ICreatorRepository _creatorRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly ILogger<CreatorService> _logger;
    private readonly IEmailService _emailService;
    private readonly IVerificationService _verificationService;
    private readonly IMediaService _mediaService;
    private readonly IWalletService _walletService;

    public CreatorService(IRepository repository,
        ICreatorRepository creatorRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IPaymentGatewayService paymentGatewayService,
        UserManager<ApplicationUser> userManager,
        ILogger<CreatorService> logger,
        IEmailService emailService,
        IVerificationService verificationService,
        IMediaService mediaService,
        IPostRepository postRepository,
        IUserRepository userRepository,
        IRegularUserRepository regularUserRepository,
        IWalletService walletService,
        IHttpContextAccessor httpContextAccessor)
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
        _postRepository = postRepository;
        _userRepository = userRepository;
        _regularUserRepository = regularUserRepository;
        _walletService = walletService;
        _httpContextAccessor = httpContextAccessor;
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
                    FullName = c.ApplicationUser!.FullName!,
                    Industry = c.ExclusiveDeals.Select(ed => ed.Industry).FirstOrDefault()!,
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

    public async Task<ServerResponse<PostResponseDto>> CreatePostAsync(PostDto postDto, PostVisibility visibility,
    string userId, List<IFormFile> mediaFiles)
    {
        // Check if the media file count exceeds the limit
        if (mediaFiles.Count > 10)
        {
            return new ServerResponse<PostResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "A maximum of 10 media files (images or videos) is allowed per post."
            };
        }

        var creator = await _repository.GetAll<Creator>().FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
        if (creator == null)
        {
            return new ServerResponse<PostResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Creator not found."
            };
        }

        // Create a new Post entity
        var post = new Post
        {
            CreatorId = creator.Id,
            Caption = postDto.Caption,
            Location = postDto.Location,
            Visibility = visibility,
            CreatedAt = DateTime.UtcNow
        };

        // Upload and save each media file
        var mediaUrls = new List<string>();
        foreach (var file in mediaFiles)
        {
            var fileType = DetermineFileType(file);
            Dictionary<string, string> uploadResponse;

            if (fileType == "image")
            {
                uploadResponse = await _mediaService.UploadImageAsync(file, "images-folder");
            }
            else if (fileType == "video")
            {
                uploadResponse = await _mediaService.UploadVideoAsync(file, "videos-folder");
            }
            else
            {
                return new ServerResponse<PostResponseDto>
                {
                    IsSuccessful = false,
                    ResponseCode = "400",
                    ResponseMessage = "Unsupported file type."
                };
            }

            if (uploadResponse["Code"] == "200")
            {
                var media = new Media
                {
                    PostId = post.Id,  
                    Url = uploadResponse["Url"],
                    FileType = fileType,
                    CreatedAt = DateTime.UtcNow
                };

                post.MediaFiles.Add(media);
                mediaUrls.Add(uploadResponse["Url"]);
            }
            else
            {
                return new ServerResponse<PostResponseDto>
                {
                    IsSuccessful = false,
                    ResponseCode = "400",
                    ResponseMessage = "Failed to upload file."
                };
            }
        }

        // Save the post and associated media files in a single transaction
        _repository.Add(post);
        await _unitOfWork.SaveChangesAsync();

        // Return response with media URLs
        return new ServerResponse<PostResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Post created successfully.",
            Data = new PostResponseDto
            {
                PostId = post.Id,
                CreatorName = creator.ApplicationUser?.FullName,
                CreatorId = creator.Id,
                Location = post.Location,
                Visibility = post.Visibility.ToString(),
                Caption = post.Caption,
                MediaUrls = mediaUrls,
                CreatedAt = post.CreatedAt
            }
        };
    }



    public string DetermineFileType(IFormFile file)
    {
        // Use MIME type or file extension to determine if the file is an image or video
        var imageMimeTypes = new List<string> { "image/jpeg", "image/jpg", "image/png" };
        var videoMimeTypes = new List<string> { "video/mp4", "video/avi", "video/mov" };

        if (imageMimeTypes.Contains(file.ContentType))
        {
            return "image";
        }
        else if (videoMimeTypes.Contains(file.ContentType))
        {
            return "video";
        }

        // Check file extension as a fallback
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
        {
            return "image";
        }
        else if (extension == ".mp4" || extension == ".avi" || extension == ".mov")
        {
            return "video";
        }

        // Default to image if the type cannot be determined
        return "image";
    }
  
    public async Task<ServerResponse<PostResponseDto>> UpdatePostAsync(string postId, UpdatePostDto postDto,
     PostVisibility visibility, string userId, List<IFormFile> mediaFiles)
    {
        _logger.LogInformation("Updating post with Post ID: {PostId} for User ID: {UserId}", postId, userId);

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

        var post = _repository.GetAll<Post>().FirstOrDefault(x => x.Id == postId);
        if (post == null)
        {
            _logger.LogWarning("Post with ID: {PostId} not found.", postId);
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

        // Enforce media file limit
        if (mediaFiles.Count > 10)
        {
            return new ServerResponse<PostResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "A maximum of 10 media files (images or videos) is allowed per post."
            };
        }

        // Process each media file
        foreach (var mediaFile in mediaFiles)
        {
            var fileType = DetermineFileType(mediaFile);
            Dictionary<string, string> uploadResponse;

            if (fileType == "image")
            {
                uploadResponse = await _mediaService.UploadImageAsync(mediaFile, "images-folder");
            }
            else if (fileType == "video")
            {
                uploadResponse = await _mediaService.UploadVideoAsync(mediaFile, "videos-folder");
            }
            else
            {
                return new ServerResponse<PostResponseDto>
                {
                    IsSuccessful = false,
                    ResponseCode = "400",
                    ResponseMessage = "Unsupported file type."
                };
            }

            if (uploadResponse["Code"] == "200")
            {
                var media = new Media
                {
                    PostId = post.Id,
                    Url = uploadResponse["Url"],
                    FileType = fileType,
                    CreatedAt = DateTime.UtcNow
                };
                post.MediaFiles.Add(media);
            }
            else
            {
                _logger.LogWarning("Failed to upload media file for post ID: {PostId}", postId);
            }
        }

        // Update post details
        post.Caption = postDto.Caption ?? post.Caption;
        post.Location = postDto.Location ?? post.Location;
        post.Visibility = visibility;
        post.UpdatedAt = DateTimeOffset.UtcNow;

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
                MediaUrls = post.MediaFiles.Select(m => m.Url).ToList(),
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
            _logger.LogError(ex, "Error updating post with Post ID: {PostId}", postId);
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
        _logger.LogInformation("Attempting to soft delete post with Post ID: {PostId} for User ID: {UserId}", postId, userId);

        // Check if the user exists
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

        // Fetch the post
        var post = await _repository.GetAll<Post>().FirstOrDefaultAsync(x => x.Id == postId && !x.IsDeleted); // Ensure the post is not already soft deleted
        if (post == null)
        {
            _logger.LogWarning("Post with ID: {PostId} not found or already deleted.", postId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Post not found or already deleted.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Post not found or already deleted."
                }
            };
        }

        // Check if the user is the creator of the post
        var creator = await _repository.FindByCondition<Creator>(c => c.Id == post.CreatorId && c.ApplicationUserId == userId);
        if (creator == null)
        {
            _logger.LogWarning("User with ID: {UserId} is not authorized to delete this post.", userId);
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
            // Perform soft delete by setting the IsDeleted flag
            post.IsDeleted = true;
            _repository.Update(post); // Update the post entity with the IsDeleted flag

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Post with ID: {PostId} soft deleted successfully for User ID: {UserId}", postId, userId);
            return new ServerResponse<object>
            {
                IsSuccessful = true,
                ResponseCode = "00",
                ResponseMessage = "Post deleted successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting post with Post ID: {PostId}", postId);
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
            ResponseMessage = "Comment added to post successfully.",
            Data = commentResponse
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
            ResponseMessage = "Comment liked successfully.",
            Data = likeResponse
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
            ResponseMessage = "Post liked successfully.",
            Data = likeResponse
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

        if (string.IsNullOrEmpty(request.RegularUserId))
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
        var user = await _repository.GetById<ApplicationUser>(request.RegularUserId);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found for request ID: {RequestId}", request.RegularUserId, requestId);
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
            ResponseMessage = "Request handled successfully.",
            Data = requestResponse
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
        if (string.IsNullOrEmpty(request.RegularUserId))
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
        var regularUser = await _regularUserRepository.GetByIdWithApplicationUser(request.RegularUserId);
        if (regularUser == null || regularUser.ApplicationUser == null)
        {
            _logger.LogWarning("User with ID: {UserId} or associated ApplicationUser not found.", request.RegularUserId);
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User or associated ApplicationUser not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "User or associated ApplicationUser not found."
                }
            };
        }

        // Check if the creator is sending the video
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        if (user == null)
        {
            _logger.LogWarning("User not found.");
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "User.Error",
                    ResponseMessage = "User not found"
                }
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        //_logger.LogInformation("User roles for {UserId}: {Roles}", user.Id, string.Join(", ", roles));
        //var creator = await _repository.GetById<ApplicationUser>(request.CreatorId); // Assuming you have the CreatorId in the Request model
        if (roles == null || !roles.Contains("Creator"))
        {
            _logger.LogWarning("User with ID: {UserId} is not authorized to send videos.", request.RegularUserId);
            return new ServerResponse<VideoResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "403",
                ResponseMessage = "You are not authorized to send videos.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "403",
                    ResponseMessage = "You are not authorized to send videos."
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
                    ResponseMessage = "Failed to upload video.",
                    ResponseDescription= "Invalid type.",
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
            CreatorName = user.FullName,
            SentToRegularUser = regularUser.ApplicationUser!.FullName,
        };

        return new ServerResponse<VideoResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Video sent successfully.",
            Data = videoResponse
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
        user.Wallet.Balance = amount;
        _repository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Withdrawal successful for creator with ID: {CreatorId}, amount: {Amount}, currency: {Currency}", creatorId, amount, currency);

        // Prepare the response DTO
        var withdrawResponse = new WithdrawResponseDto
        {
            CreatorId = creatorId,
            Amount = amount,
            Currency = currency,
            RemainingBalance = user.Wallet.Balance,
            TransactionDate = DateTimeOffset.UtcNow
        };

        return new ServerResponse<WithdrawResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Withdrawal successful.",
            Data = withdrawResponse
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

    public async Task<ServerResponse<IEnumerable<PostResponseDto>>> GetPostsByCreatorAsync(string applicationUserId)
    {
        _logger.LogInformation("Getting posts for creator with ApplicationUser ID: {ApplicationUserId}", applicationUserId);

        // Step 1: Validate and retrieve Creator based on ApplicationUserId
        if (string.IsNullOrEmpty(applicationUserId))
        {
            _logger.LogWarning("Invalid ApplicationUser ID: {ApplicationUserId}", applicationUserId);
            return new ServerResponse<IEnumerable<PostResponseDto>>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "ApplicationUser ID cannot be null or empty.",
                Data = null!
            };
        }

        // Step 2: Retrieve creator from Creator repository
        var creator = await _repository.GetAll<Creator>()
                                              .Where(c => c.ApplicationUserId == applicationUserId)
                                              .FirstOrDefaultAsync();

        if (creator == null)
        {
            _logger.LogWarning("No creator found for ApplicationUser ID: {ApplicationUserId}", applicationUserId);
            return new ServerResponse<IEnumerable<PostResponseDto>>
            {
                IsSuccessful = true,  // Return success with an empty list
                ResponseCode = "00",
                ResponseMessage = "No creator found.",
                Data = new List<PostResponseDto>()  // Return empty list
            };
        }

        // Step 3: Use CreatorId to retrieve posts from PostRepository
        var posts = await _postRepository.GetAllPosts()
                             .Where(p => p.CreatorId == creator.Id)
                             .ToListAsync();

        if (posts == null || !posts.Any())
        {
            _logger.LogWarning("No posts found for creator ID: {CreatorId}", creator.Id);
            return new ServerResponse<IEnumerable<PostResponseDto>>
            {
                IsSuccessful = true,  // Return success even if no posts found
                ResponseCode = "00",
                ResponseMessage = "No posts found for this creator.",
                Data = new List<PostResponseDto>()  // Return empty list
            };
        }

        _logger.LogInformation("Number of posts found for creator ID {CreatorId}: {PostCount}", creator.Id, posts.Count);

        // Step 4: Map posts to PostResponseDto
        var postResponses = posts.Select(p => new PostResponseDto
        {
            PostId = p.Id,
            CreatorId = p.CreatorId,
            CreatorName = p.Creator?.ApplicationUser?.FullName ?? "Unknown",
            Caption = p.Caption,
            MediaUrls = p.MediaFiles.Select(m => m.Url).ToList()!,
            Location = p.Location,
            Visibility = p.Visibility.ToString(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();

        return new ServerResponse<IEnumerable<PostResponseDto>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Posts retrieved successfully.",
            Data = postResponses
        };
    }

    public async Task<ServerResponse<CreatorRateResponseDto>> UpdateCreatorRatesAsync(UpdateCreatorRateDto dto, string userId)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ServerResponse<CreatorRateResponseDto>
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

        // Find the creator associated with the user
        var creator = _repository.GetAll<Creator>()
            .Include(c => c.Rates)
            .FirstOrDefault(x => x.ApplicationUserId == user.Id);

        if (creator == null)
        {
            return new ServerResponse<CreatorRateResponseDto>
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

        // Limit rates to 10 entries
        if (dto.Rates.Count > 10)
        {
            return new ServerResponse<CreatorRateResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Cannot add more than 10 rates.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "Maximum of 10 rates allowed."
                }
            };
        }

        // Clear existing rates and add new ones
        creator.Rates.Clear();
        foreach (var rateDto in dto.Rates)
        {
            creator.Rates.Add(new Rate
            {
                Type = rateDto.Type,
                Price = rateDto.Price
            });
        }

        // Save the updates to the database
        _repository.Update(creator);
        await _unitOfWork.SaveChangesAsync();

        // Prepare the response DTO
        var creatorProfileResponse = new CreatorRateResponseDto
        {
            CreatorId = creator.Id,
            Rates = creator.Rates.Select(rate => new RateDto
            {
                RateId = rate.Id, 
                Type = rate.Type!,
                Price = rate.Price
            }).ToList()
        };

        return new ServerResponse<CreatorRateResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Creator's profile updated successfully.",
            Data = creatorProfileResponse
        };
    }


    public async Task<ServerResponse<string>> DeleteCreatorRateAsync(string rateId, string userId)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ServerResponse<string>
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

        // Find the creator associated with the user
        var creator = _repository.GetAll<Creator>()
            .Include(c => c.Rates) // Include Rates
            .FirstOrDefault(c => c.ApplicationUserId == user.Id);

        if (creator == null)
        {
            return new ServerResponse<string>
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

        // Find the rate by ID within the creator's rates
        var rate = creator.Rates.FirstOrDefault(r => r.Id == rateId);
        if (rate == null)
        {
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Rate not found.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "404",
                    ResponseMessage = "Rate not found."
                }
            };
        }

        // Remove the rate
        creator.Rates.Remove(rate);
        _repository.Update(creator);
        await _unitOfWork.SaveChangesAsync();

        return new ServerResponse<string>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Rate deleted successfully.",
            Data = "Rate deleted successfully."
        };
    }


    //public async Task<ServerResponse<CreatorProfileResponseDto>> UpdateCreatorSetUpRatesAsync(UpdateCreatorProfileDto dto, string userId)
    //{
    //    // Find the user by ID
    //    var user = await _userManager.FindByIdAsync(userId);

    //    if (user == null)
    //        return new ServerResponse<CreatorProfileResponseDto>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "404",
    //            ResponseMessage = "User not found.",
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "404",
    //                ResponseMessage = "User not found."
    //            }
    //        };

    //    // Find the creator associated with the user
    //    var creator = _repository.GetAll<Creator>()
    //        .FirstOrDefault(x => x.ApplicationUserId == user.Id);

    //    if (creator == null)
    //        return new ServerResponse<CreatorProfileResponseDto>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "404",
    //            ResponseMessage = "Creator not found.",
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "404",
    //                ResponseMessage = "Creator not found."
    //            }
    //        };

    //    // Update the creator's details
    //    creator.SimpleAdvert = dto.SimpleAdvert ?? creator.SimpleAdvert;
    //    creator.WearBrand = dto.WearBrand ?? creator.WearBrand;
    //    creator.SongAdvert = dto.SongAdvert ?? creator.SongAdvert;
    //    creator.Request = dto.Request ?? creator.Request;        

    //    // Save the updates to the database
    //    _repository.Update(creator);
    //    await _userManager.UpdateAsync(user);
    //    await _unitOfWork.SaveChangesAsync();

    //    // Prepare the response DTO
    //    var creatorProfileResponse = new CreatorProfileResponseDto
    //    {           
    //        SimpleAdvert = creator.SimpleAdvert,
    //        WearBrand = creator.WearBrand,
    //        SongAdvert = creator.SongAdvert,
    //        Request = creator.Request,
    //    };

    //    return new ServerResponse<CreatorProfileResponseDto>
    //    {
    //        IsSuccessful = true,
    //        ResponseCode = "00",
    //        ResponseMessage = "Creator's profile updated successfully.",
    //        Data = creatorProfileResponse
    //    };

    //}

    public async Task<ServerResponse<PaginatorDto<IEnumerable<FilterCreatorDto>>>> SearchCreatorsAsync(
    decimal? minPrice, decimal? maxPrice, string? location, string? industry, string? keyword, PaginationFilter paginationFilter)
    {
        location = location?.Trim();
        industry = industry?.Trim();
        keyword = keyword?.Trim();

        _logger.LogInformation(new EventId(),
            "Searching for creators with filters - MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, Location: {Location}, Industry: {Industry}, Keyword: {Keyword}",
            minPrice, maxPrice, location, industry, keyword);

        var query = _repository.GetAll<Creator>()
            .Include(c => c.ApplicationUser)
            .Where(c => c.ApplicationUser != null);

        // Log initial creator count
        _logger.LogInformation("Total creators found before applying filters: {CreatorCount}", await query.CountAsync());

        // Apply filters
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
            query = query.Where(c => c.ApplicationUser!.Location!.ToLower().Contains(location.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(industry))
        {
            query = query.Where(c => c.ApplicationUser!.Occupation!.ToLower().Contains(industry.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(c =>
                (c.ApplicationUser!.FirstName.ToLower().Contains(keyword.ToLower()) ||
                 c.ApplicationUser!.LastName.ToLower().Contains(keyword.ToLower()) ||
                 c.ApplicationUser.Location!.ToLower().Contains(keyword.ToLower()) ||
                 c.ApplicationUser.Occupation!.ToLower().Contains(keyword.ToLower()) ||
                 c.Price.ToString().Contains(keyword)));
        }

        // Apply pagination
        var paginatedCreators = await query.PaginateAsync(paginationFilter);

        // Check if no creators found and return a successful response with an empty list
        if (!paginatedCreators.PageItems!.Any())
        {
            _logger.LogInformation("No creators found after applying filters");
            return new ServerResponse<PaginatorDto<IEnumerable<FilterCreatorDto>>>
            {
                IsSuccessful = true,  // Still return a successful response
                ResponseCode = "00",
                ResponseMessage = "No creators found matching the search criteria.",
                Data = new PaginatorDto<IEnumerable<FilterCreatorDto>>
                {
                    CurrentPage = paginatedCreators.CurrentPage,
                    PageSize = paginatedCreators.PageSize,
                    NumberOfPages = paginatedCreators.NumberOfPages,
                    PageItems = new List<FilterCreatorDto>()  // Return an empty list
                }
            };
        }

        var creatorDtos = paginatedCreators.PageItems!.Select(c => new FilterCreatorDto
        {
            CreatorId = c.Id,
            FullName = $"{c.ApplicationUser?.FirstName ?? "N/A"} {c.ApplicationUser?.LastName ?? "N/A"}",
            Price = c.Price,
            Location = c.ApplicationUser?.Location ?? "N/A",
            ImageUrl = c.ApplicationUser?.ImageUrl ?? "N/A",
            Industry = c.ApplicationUser?.Occupation ?? "N/A"
        }).ToList();

        _logger.LogInformation("Found {CreatorCount} creators matching the filters", creatorDtos.Count);

        return new ServerResponse<PaginatorDto<IEnumerable<FilterCreatorDto>>>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Creators found successfully.",
            Data = new PaginatorDto<IEnumerable<FilterCreatorDto>>
            {
                CurrentPage = paginatedCreators.CurrentPage,
                PageSize = paginatedCreators.PageSize,
                NumberOfPages = paginatedCreators.NumberOfPages,
                PageItems = creatorDtos
            }
        };
    }

    public async Task<ServerResponse<object>> WithdrawFundsToBankAccountAsync(string userId, decimal amount, string bankCardId)
    {
        // Check if the user exists
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for ID: {UserId}", userId);
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "06",
                    ResponseMessage = "User not found"
                }
            };
        }

        // Check if the user is a creator
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Creator"))
        {
            _logger.LogWarning("User is not a creator.");
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "Creator.Error",
                    ResponseMessage = "Creator not found",
                    ResponseDescription = "Only Creators can withdraw funds to their bank accounts.",
                }
            };
        }

        // Check if the amount is valid (e.g., greater than zero)
        if (amount <= 0)
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "InvalidAmount.Error",
                    ResponseMessage = "Invalid withdrawal amount",
                    ResponseDescription = "The amount must be greater than zero."
                }
            };
        }

        // Check if the user has sufficient funds
        var walletBalance = await _walletService.GetBalanceAsync(userId);
        if (walletBalance < amount)
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "InsufficientFunds.Error",
                    ResponseMessage = "Insufficient funds",
                    ResponseDescription = "You do not have enough funds in your wallet to complete this withdrawal."
                }
            };
        }

        // Process the withdrawal to the bank account via card
        var withdrawalResult = await _walletService.WithdrawToBankAccountAsync(userId, amount, bankCardId);
        if (!withdrawalResult.IsSuccessful)
        {
            return new ServerResponse<object>
            {
                IsSuccessful = false,
                ResponseCode = withdrawalResult.ResponseCode,
                ResponseMessage = withdrawalResult.ResponseMessage,
                ErrorResponse = withdrawalResult.ErrorResponse
            };
        }

        // Update the wallet balance after successful withdrawal
        await _userRepository.UpdateWalletBalanceAsync(userId, -amount);
        _logger.LogInformation("User with ID: {UserId} withdrew amount: {Amount} to bank account.", userId, amount);

        // Return success response
        return new ServerResponse<object>
        {
            IsSuccessful = true,
            ResponseCode = "00",
            ResponseMessage = "Withdrawal to bank account initiated successfully.",
            Data = new
            {
                UserId = userId,
                Amount = amount,
                BankCardId = bankCardId
            }
        };
    }



    private string GenerateVerificationCode()
    {
        // Generate a random 5-digit number as verification code
        var random = new Random();
        return random.Next(10000, 99999).ToString();
    }


    //public async Task<ServerResponse<PostResponseDto>> CreatePostAsync(PostDto postDto, PostVisibility visibility, string userId, IFormFile photo)
    //{
    //    _logger.LogInformation("Creating post for creator with User ID: {UserId}", userId);

    //    // Check if the creator exists in the database by UserId
    //    var creator = await _repository.FindByCondition<Creator>(c => c.ApplicationUserId == userId);
    //    if (creator == null)
    //    {
    //        _logger.LogWarning("Creator with User ID: {UserId} not found.", userId);
    //        return new ServerResponse<PostResponseDto>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "404",
    //            ResponseMessage = "Creator does not exist.",
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "404",
    //                ResponseMessage = "Creator not found."
    //            }
    //        };
    //    }

    //    // Upload image to Cloudinary if a photo is provided
    //    string mediaUrl = null!;
    //    if (photo != null)
    //    {
    //        var uploadResult = await _mediaService.UploadImageAsync(photo, "post_images"); // Assuming you have a 'post_images' folder in Cloudinary
    //        if (uploadResult["Code"] != "200")
    //        {
    //            return new ServerResponse<PostResponseDto>
    //            {
    //                IsSuccessful = false,
    //                ResponseCode = "400",
    //                ResponseMessage = "Image upload failed.",
    //                ErrorResponse = new ErrorResponse
    //                {
    //                    ResponseCode = "400",
    //                    ResponseMessage = "Failed to upload the image."
    //                }
    //            };
    //        }
    //        mediaUrl = uploadResult["Url"];
    //    }

    //    // Create the new post
    //    var post = new Post
    //    {
    //        CreatorId = creator.Id,
    //        Caption = postDto.Caption,
    //        MediaUrl = mediaUrl!,  // Use the uploaded image URL
    //        Location = postDto.Location,
    //        Visibility = visibility,
    //        CreatedAt = DateTimeOffset.UtcNow,
    //        UpdatedAt = DateTimeOffset.UtcNow
    //    };

    //    _logger.LogInformation("Post object created: {Post}", post);

    //    try
    //    {
    //        await _repository.Add(post);
    //        await _unitOfWork.SaveChangesAsync();

    //        _logger.LogInformation("Post created successfully for creator with User ID: {UserId}", userId);

    //        var postResponse = new PostResponseDto
    //        {
    //            PostId = post.Id,
    //            CreatorId = creator.Id,
    //            CreatorName = creator.ApplicationUser?.FullName,
    //            Caption = post.Caption,
    //            MediaUrl = post.MediaUrl,
    //            Location = post.Location,
    //            Visibility = post.Visibility.ToString(),
    //            CreatedAt = post.CreatedAt,
    //            UpdatedAt = post.UpdatedAt
    //        };

    //        return new ServerResponse<PostResponseDto>
    //        {
    //            IsSuccessful = true,
    //            ResponseCode = "00",
    //            ResponseMessage = "Post created successfully.",
    //            Data = postResponse
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error creating post for creator with User ID: {UserId}", userId);
    //        return new ServerResponse<PostResponseDto>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "500",
    //            ResponseMessage = "An error occurred while creating the post.",
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "500",
    //                ResponseMessage = "Internal server error",
    //                ResponseDescription = ex.Message
    //            }
    //        };
    //    }
    //}

    //public async Task<ServerResponse<PostResponseDto>> CreatePostAsync(PostDto postDto, PostVisibility visibility, string userId, IFormFile photo, IFormFile video)
    //{
    //    _logger.LogInformation("Creating post for creator with User ID: {UserId}", userId);

    //    // Check if the creator exists in the database by UserId
    //    var creator = await _repository.FindByCondition<Creator>(c => c.ApplicationUserId == userId);
    //    if (creator == null)
    //    {
    //        _logger.LogWarning("Creator with User ID: {UserId} not found.", userId);
    //        return new ServerResponse<PostResponseDto>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "404",
    //            ResponseMessage = "Creator does not exist.",
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "404",
    //                ResponseMessage = "Creator not found."
    //            }
    //        };
    //    }

    //    // Initialize media URL for either photo or video
    //    string mediaUrl = null;

    //    // Upload image to Cloudinary if a photo is provided
    //    if (photo != null)
    //    {
    //        var uploadResult = await _mediaService.UploadImageAsync(photo, "post_images"); // Assuming you have a 'post_images' folder in Cloudinary
    //        if (uploadResult["Code"] != "200")
    //        {
    //            return new ServerResponse<PostResponseDto>
    //            {
    //                IsSuccessful = false,
    //                ResponseCode = "400",
    //                ResponseMessage = "Image upload failed.",
    //                ErrorResponse = new ErrorResponse
    //                {
    //                    ResponseCode = "400",
    //                    ResponseMessage = "Failed to upload the image."
    //                }
    //            };
    //        }
    //        mediaUrl = uploadResult["Url"];
    //    }

    //    // Upload video to Cloudinary if a video is provided
    //    if (video != null)
    //    {
    //        var uploadResult = await _mediaService.UploadVideoAsync(video, "post_videos"); // Assuming you have a 'post_videos' folder in Cloudinary
    //        if (uploadResult["Code"] != "200")
    //        {
    //            return new ServerResponse<PostResponseDto>
    //            {
    //                IsSuccessful = false,
    //                ResponseCode = "400",
    //                ResponseMessage = "Video upload failed.",
    //                ErrorResponse = new ErrorResponse
    //                {
    //                    ResponseCode = "400",
    //                    ResponseMessage = "Failed to upload the video."
    //                }
    //            };
    //        }
    //        mediaUrl = uploadResult["Url"]; // Use the video URL if uploaded successfully
    //    }

    //    // Create the new post
    //    var post = new Post
    //    {
    //        CreatorId = creator.Id,
    //        Caption = postDto.Caption,
    //        MediaUrl = mediaUrl,  // Use the uploaded media URL (image or video)
    //        Location = postDto.Location,
    //        Visibility = visibility,
    //        CreatedAt = DateTimeOffset.UtcNow,
    //        UpdatedAt = DateTimeOffset.UtcNow
    //    };

    //    _logger.LogInformation("Post object created: {Post}", post);

    //    try
    //    {
    //        await _repository.Add(post);
    //        await _unitOfWork.SaveChangesAsync();

    //        _logger.LogInformation("Post created successfully for creator with User ID: {UserId}", userId);

    //        var postResponse = new PostResponseDto
    //        {
    //            PostId = post.Id,
    //            CreatorId = creator.Id,
    //            CreatorName = creator.ApplicationUser?.FullName,
    //            Caption = post.Caption,
    //            MediaUrl = post.MediaUrl,
    //            Location = post.Location,
    //            Visibility = post.Visibility.ToString(),
    //            CreatedAt = post.CreatedAt,
    //            UpdatedAt = post.UpdatedAt
    //        };

    //        return new ServerResponse<PostResponseDto>
    //        {
    //            IsSuccessful = true,
    //            ResponseCode = "00",
    //            ResponseMessage = "Post created successfully.",
    //            Data = postResponse
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error creating post for creator with User ID: {UserId}", userId);
    //        return new ServerResponse<PostResponseDto>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "500",
    //            ResponseMessage = "An error occurred while creating the post.",
    //            ErrorResponse = new ErrorResponse
    //            {
    //                ResponseCode = "500",
    //                ResponseMessage = "Internal server error",
    //                ResponseDescription = ex.Message
    //            }
    //        };
    //    }
    //}

}
