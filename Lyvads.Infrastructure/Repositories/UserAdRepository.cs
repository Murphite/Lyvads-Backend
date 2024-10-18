using Lyvads.Domain.Repositories;
using Lyvads.Domain.Entities;
using Lyvads.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class UserAdRepository : IUserAdRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserAdRepository> _logger;

    public UserAdRepository(AppDbContext context,
        ILogger<UserAdRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<UserAd>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all user ads from the database...");
        try
        {
            var ads = await _context.UserAds.ToListAsync();
            _logger.LogInformation("Successfully retrieved all user ads.");
            return ads;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occurred while fetching user ads: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<UserAd> GetByIdAsync(string adId)
    {
        _logger.LogInformation("Fetching user ad with ID: {AdId}", adId);
        try
        {
            var ad = await _context.UserAds.FindAsync(adId);
            if (ad != null)
            {
                _logger.LogInformation("Successfully retrieved user ad with ID: {AdId}", adId);
            }
            else
            {
                _logger.LogWarning("User ad not found with ID: {AdId}", adId);
            }
            return ad!;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occurred while fetching user ad with ID {AdId}: {Message}", adId, ex.Message);
            throw;
        }
    }

    public async Task UpdateAsync(UserAd ad)
    {
        _logger.LogInformation("Updating user ad with ID: {AdId}", ad.Id);
        try
        {
            _context.UserAds.Update(ad);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully updated user ad with ID: {AdId}", ad.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occurred while updating user ad with ID {AdId}: {Message}", ad.Id, ex.Message);
            throw;
        }
    }

}