using Lyvads.Infrastructure.Persistence;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lyvads.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApplicationUser> GetUserByIdAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        return user;
    }

    public async Task AddCommentAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
    }

    public async Task AddLikeAsync(Like like)
    {
        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();
    }

    public async Task<Like> GetLikeAsync(string userId, string contentId)
    {
        var like = await _context.Likes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ContentId == contentId);

        if (like == null)
        {
            throw new Exception("Like not found");
        }

        return like;
    }


    public async Task RemoveLikeAsync(Like like)
    {
        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();
    }



    public async Task UpdateWalletBalanceAsync(string userId, decimal amount)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.WalletBalance += amount;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }


}
