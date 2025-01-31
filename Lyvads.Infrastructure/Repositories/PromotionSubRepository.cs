
using Microsoft.Extensions.Logging;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Lyvads.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class PromotionSubRepository : IPromotionSubRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<PromotionSubRepository> _logger;

    public PromotionSubRepository(AppDbContext context, ILogger<PromotionSubRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PromotionSubscription> GetByPaymentReferenceAsync(string paymentReference)
    {
        // Assuming you are using Entity Framework to access your database
        return await _context.PromotionSubscriptions
            .Where(sub => sub.PaymentReference == paymentReference)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(PromotionSubscription subscription)
    {
        // Check if the subscription exists in the context
        var existingSubscription = await _context.PromotionSubscriptions
            .Where(sub => sub.Id == subscription.Id)
            .FirstOrDefaultAsync();

        if (existingSubscription != null)
        {
            // Update the fields as needed (e.g., IsActive)
            existingSubscription.IsActive = true;  // Force IsActive to true, you can adjust logic as needed
            existingSubscription.ExpiryDate = subscription.ExpiryDate; // Example: Update expiry date if needed
            existingSubscription.SubscriptionDate = subscription.SubscriptionDate; // Example: Update subscription date if needed

            // Mark the entity as modified if it is tracked
            _context.PromotionSubscriptions.Update(existingSubscription);

            // Save changes to the database
            await _context.SaveChangesAsync();
        }
        else
        {
            // If the subscription does not exist, you can either throw an exception or log it.
            _logger.LogWarning("Subscription with ID {SubscriptionId} not found for update.", subscription.Id);
        }
    }


    public async Task AddAsync(PromotionSubscription subscription)
    {
        await _context.PromotionSubscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PromotionSubscription>> GetAllSubscriptionsAsync()
    {
        return await _context.PromotionSubscriptions
            .Include(sub => sub.Creator)
            .Include(sub => sub.PromotionPlan)
            .ToListAsync();
    }

    public async Task<Creator?> GetCreatorByIdAsync(string creatorId)
    {
        return await _context.Creators
            .Include(c => c.ApplicationUser)
            .FirstOrDefaultAsync(c => c.Id == creatorId);
    }

    public async Task<PaginatorDto<IEnumerable<PromotionSubscription>>> GetPaginatedSubscriptionsAsync(PaginationFilter paginationFilter)
    {
        var query = _context.PromotionSubscriptions.AsQueryable();

        // Apply pagination logic
        var totalRecords = await query.CountAsync();
        var subscriptions = await query
            .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
            .Take(paginationFilter.PageSize)
            .ToListAsync();

        return new PaginatorDto<IEnumerable<PromotionSubscription>>
        {
            CurrentPage = paginationFilter.PageNumber,
            PageSize = paginationFilter.PageSize,
            NumberOfPages = (int)Math.Ceiling((double)totalRecords / paginationFilter.PageSize),
            PageItems = subscriptions
        };
    }

}

