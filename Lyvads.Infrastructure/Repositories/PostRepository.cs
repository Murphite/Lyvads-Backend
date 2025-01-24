
using Azure;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Lyvads.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _context;

    public PostRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Post> GetAllPosts()
    {
        return _context.Posts.AsQueryable();
    }

    public async Task<List<Post>> GetFilteredPostsAsync(List<string> followingIds)
    {
        return await _context.Posts
            .Where(p => p.Visibility == PostVisibility.Public ||
                        (p.Visibility == PostVisibility.Private && followingIds.Contains(p.CreatorId)))
            .ToListAsync();
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
