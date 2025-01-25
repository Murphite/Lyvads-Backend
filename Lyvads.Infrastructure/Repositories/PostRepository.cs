
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

    public PostRepository(
        AppDbContext context, 
        ILogger<PostRepository> logger)
    {
        _context = context;
        _logger = logger;
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
