

using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Lyvads.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class PromotionPlanRepository : IPromotionPlanRepository
{
    private readonly AppDbContext _context;

    public PromotionPlanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PromotionPlan promotionPlan)
    {
        await _context.PromotionPlans.AddAsync(promotionPlan);
        await _context.SaveChangesAsync();
    }

    public async Task<PromotionPlan?> GetByIdAsync(string planId)
    {
        return await _context.PromotionPlans.FindAsync(planId);
    }

    public async Task<List<PromotionPlan>> GetAllAsync()
    {
        return await _context.PromotionPlans.ToListAsync();
    }

    public async Task<PaginatorDto<IEnumerable<PromotionPlan>>> GetPaginatedPlansAsync(PaginationFilter paginationFilter)
    {
        var query = _context.PromotionPlans.AsQueryable();

        // Apply pagination logic
        var totalRecords = await query.CountAsync();
        var plans = await query
            .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
            .Take(paginationFilter.PageSize)
            .ToListAsync();

        return new PaginatorDto<IEnumerable<PromotionPlan>>
        {
            CurrentPage = paginationFilter.PageNumber,
            PageSize = paginationFilter.PageSize,
            NumberOfPages = (int)Math.Ceiling((double)totalRecords / paginationFilter.PageSize),
            PageItems = plans
        };
    }

}
