
using Azure;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Persistence;
using Lyvads.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace Lyvads.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<PostRepository> _logger;
    private readonly IRepository _repository;

    public PostRepository(
        AppDbContext context, 
        ILogger<PostRepository> logger,
        IRepository repository)
    {
        _context = context;
        _logger = logger;
        _repository = repository;
    }

    public async Task<Post> GetPostWithMediaAsync(string postId)
    {
        // Query the database to fetch the post and include its media files
        var post = await _context.Posts
            .Include(p => p.MediaFiles) // Include media files
            .Include(p => p.Creator)   // Include creator information if necessary
            .ThenInclude(c => c.ApplicationUser) // Include application user details
            .FirstOrDefaultAsync(p => p.Id == postId);

        return post!;
    }

    public async Task<IEnumerable<Like>> GetLikesByUserAndPostsAsync(string userId, List<string> postIds)
    {
        return await _repository.QueryFindByCondition<Like>(l => postIds.Contains(l.PostId) && l.UserId == userId).ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetCommentsByUserAndPostsAsync(string userId, List<string> postIds)
    {
        return await _repository.QueryFindByCondition<Comment>(c =>
            postIds.Contains(c.PostId) &&
            (c.RegularUserId == userId || c.ApplicationUserId == userId))
            .ToListAsync();
    }

    public async Task<List<Comment>> GetRepliesByUserAndPostsAsync(string userId, List<string> postIds)
    {
        return await _context.Comments
            .Where(c => c.ParentCommentId != null // Ensure it's a reply
                        && (c.RegularUserId == userId || c.ApplicationUserId == userId) // Check if user made the reply
                        && postIds.Contains(c.PostId)) // Ensure reply belongs to the given posts
            .Include(c => c.Post) // Include post details if needed
            .ToListAsync();
    }

    public IQueryable<Post> GetAllPosts()
    {
        return _context.Posts.AsQueryable();
    }

    public async Task<List<Post>> GetFilteredPostsAsync(List<string> followingIds)
    {
        return await _context.Posts
            .Include(p => p.Creator)
            .ThenInclude(c => c.ApplicationUser)
            .Include(p => p.MediaFiles)
            .Where(p => p.Visibility == PostVisibility.Public || followingIds.Contains(p.CreatorId))
            .ToListAsync();
    }

    public async Task<PaginatorDto<IEnumerable<Post>>> GetPaginatedPostsAsync(List<string> followingIds, PaginationFilter paginationFilter)
    {
        // Build the query for filtering posts
        var query = _context.Posts
            .Include(p => p.Creator)
            .ThenInclude(c => c.ApplicationUser)
            .Include(p => p.MediaFiles)
            .Where(p => p.Visibility == PostVisibility.Public || followingIds.Contains(p.CreatorId));

        // Log the total count before pagination
        var totalCount = await query.CountAsync();
        _logger.LogInformation("Total posts found before pagination: {PostCount}", totalCount);

        // Apply pagination
        var paginatedPosts = await query.PaginateAsync(paginationFilter);

        // Return paginated result
        return new PaginatorDto<IEnumerable<Post>>
        {
            CurrentPage = paginatedPosts.CurrentPage,
            PageSize = paginatedPosts.PageSize,
            NumberOfPages = paginatedPosts.NumberOfPages,
            PageItems = paginatedPosts.PageItems
        };
    }


    public async Task<PaginatedResponse<Post>> GetPaginatedPostsByCreatorAsync(string creatorId, PaginationFilter paginationFilter)
    {
        // Ensure the creatorId is not null or empty
        if (string.IsNullOrEmpty(creatorId))
        {
            throw new ArgumentException("Creator ID cannot be null or empty", nameof(creatorId));
        }

        // Retrieve the base query to filter posts by the creatorId
        var query = _context.Posts
             .Include(p => p.Creator)
            .ThenInclude(c => c.ApplicationUser)
            .Include(p => p.MediaFiles)
            //.Where(p => p.Visibility == PostVisibility.Public || followingIds.Contains(p.CreatorId));
            .Where(post => post.CreatorId == creatorId);

        // Get the total count of posts for the creator
        var totalRecords = await query.CountAsync();

        // Apply pagination (skip and take) to the query
        var paginatedPosts = await query
            .OrderByDescending(post => post.CreatedAt) // Adjust ordering as needed
            .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
            .Take(paginationFilter.PageSize)
            .ToListAsync();

        // Prepare the paginated response
        return new PaginatedResponse<Post>
        {
            Data = paginatedPosts,
            PageNumber = paginationFilter.PageNumber,
            PageSize = paginationFilter.PageSize,
            TotalRecords = totalRecords
        };
    }


    public async Task<Post> GetPostWithDetailsAsync(string postId)
    {
        if (string.IsNullOrEmpty(postId))
        {
            throw new ArgumentException("Post ID cannot be null or empty.", nameof(postId));
        }

        return await _context.Posts
            .Include(p => p.Creator)
                .ThenInclude(c => c.ApplicationUser)
            .Include(p => p.MediaFiles)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Replies)
            .FirstOrDefaultAsync(p => p.Id == postId);
    }


    public async Task<IEnumerable<Post>> GetAllAsync(
    Func<IQueryable<Post>, IIncludableQueryable<Post, object>>? include = null)
    {
        IQueryable<Post> query = _context.Posts;

        if (include != null)
        {
            query = include(query);
        }

        return await query.ToListAsync();
    }


    public async Task<ServerResponse<Post>> GetByIdAsync(string id)
    {
        var post = await _context.Posts
            .Include(p => p.Creator)
            .Include(p => p.Comments)
            .ThenInclude(c => c.ApplicationUser)
            .Include(p => p.Likes)
            .ThenInclude(l => l.ApplicationUser)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return new ServerResponse<Post>
            {
                IsSuccessful = false,
                ResponseCode = "06",
                ResponseMessage = "Post Not Found",
                ErrorResponse = new ErrorResponse
                {
                    ResponseDescription = "The post with the specified ID does not exist."
                },
                Data = null!
            };
        }

        return new ServerResponse<Post>
        {
            IsSuccessful = true,
            ResponseCode = "200",
            ResponseMessage = "Post retrieved successfully",
            Data = post
        };
    }

    public async Task<List<Comment>> GetCommentsByPostIdAsync(string postId)
    {
        return await _context.Comments
            .Where(c => c.PostId == postId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Post?> GetPostByIdAsync(string postId)
    {
        return await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);
    }


    public async Task AddAsync(Post entity)
    {
        await _context.Posts.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Post entity)
    {
        _context.Posts.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Post entity)
    {
        _context.Posts.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
