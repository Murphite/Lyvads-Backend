using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Microsoft.Extensions.Logging;

namespace Lyvads.Application.Implementations;

public class AdminPostService : IAdminPostService
{
   private readonly IPostRepository _postRepository;
    private readonly ILogger<AdminPostService> _logger;

    public AdminPostService(IPostRepository postRepository,
        ILogger<AdminPostService> logger)
    {
        _postRepository = postRepository;
        _logger = logger;
    }

    public async Task<ServerResponse<List<AdminPostDto>>> GetAllPostsAsync()
    {
        _logger.LogInformation("Fetching all posts.");

        var posts = (await _postRepository.GetAllAsync()).ToList();  // Use GetAllAsync
        var postDtos = posts.Select(post => new AdminPostDto
        {
            PostId = post.Id,
            CreatorName = post.Creator.ApplicationUser?.FirstName + " " + post.Creator.ApplicationUser?.LastName,
            Caption = post.Caption,
            DatePosted = post.CreatedAt,
            Status = post.PostStatus
        }).ToList();

        _logger.LogInformation("Successfully fetched all posts.");

        return new ServerResponse<List<AdminPostDto>>(true)
        {
            ResponseCode = "00",
            ResponseMessage = "Posts fetched successfully.",
            Data = postDtos
        };
    }

    public async Task<ServerResponse<AdminPostDetailsDto>> GetPostDetailsAsync(string postId)
    {
        _logger.LogInformation("Fetching details for post ID: {PostId}", postId);

        var post = (await _postRepository.GetByIdAsync(postId.ToString())).Data;  // Use GetByIdAsync
        if (post == null)
        {
            _logger.LogWarning("Post not found for ID: {PostId}", postId);
            return new ServerResponse<AdminPostDetailsDto>(false)
            {
                ResponseCode = "404",
                ResponseMessage = "Post not found."
            };
        }

        var postDetails = new AdminPostDetailsDto
        {
            PostId = post.Id,
            CreatorName = post.Creator?.ApplicationUser?.FirstName + " " + post.Creator?.ApplicationUser?.LastName,
            Caption = post.Caption,
            DatePosted = post.CreatedAt,
            Status = post.PostStatus,
            Comments = post.Comments.Select(c => new AdminCommentDto
            {
                UserName = c.ApplicationUser?.FirstName + " " + c.ApplicationUser?.LastName,
                Text = c.Content,
                DateCommented = c.CreatedAt
            }).ToList(),

            Likes = post.Likes.Select(l => new AdminLikeDto
            {
                UserName = l.ApplicationUser!.FirstName + " " + l.ApplicationUser.LastName
            }).ToList()
        };

        _logger.LogInformation("Successfully fetched post details for ID: {PostId}", postId);
        return new ServerResponse<AdminPostDetailsDto>(true)
        {
            ResponseCode = "00",
            ResponseMessage = "Post details fetched successfully.",
            Data = postDetails
        };
    }

    public async Task<ServerResponse<bool>> FlagPostAsync(string postId)
    {
        _logger.LogInformation("Attempting to toggle flag status for post with ID: {PostId}", postId);

        var post = (await _postRepository.GetByIdAsync(postId.ToString())).Data;
        if (post == null)
        {
            _logger.LogWarning("Post not found for ID: {PostId}", postId);
            return new ServerResponse<bool>(false)
            {
                ResponseCode = "404",
                ResponseMessage = "Post not found."
            };
        }

        // Toggle between Flagged and Live statuses
        post.PostStatus = post.PostStatus == PostStatus.Flagged ? PostStatus.Live : PostStatus.Flagged;
        await _postRepository.UpdateAsync(post);

        _logger.LogInformation("Successfully toggled status for post with ID: {PostId} to {Status}", postId, post.PostStatus);

        return new ServerResponse<bool>(true)
        {
            ResponseCode = "00",
            ResponseMessage = post.PostStatus == PostStatus.Flagged
                ? "Post flagged successfully."
                : "Post Live successfully.",
            Data = true
        };
    }

    public async Task<ServerResponse<bool>> DeletePostAsync(string postId)
    {
        _logger.LogInformation("Attempting to delete post with ID: {PostId}", postId);

        var post = (await _postRepository.GetByIdAsync(postId.ToString())).Data;  // Use GetByIdAsync
        if (post == null)
        {
            _logger.LogWarning("Post not found for ID: {PostId}", postId);
            return new ServerResponse<bool>(false)
            {
                ResponseCode = "404",
                ResponseMessage = "Post not found."
            };
        }

        await _postRepository.DeleteAsync(post);
        _logger.LogInformation("Successfully deleted post with ID: {PostId}", postId);

        return new ServerResponse<bool>(true)
        {
            ResponseCode = "00",
            ResponseMessage = "Post deleted successfully.",
            Data = true
        };
    }



}
