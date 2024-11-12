using Lyvads.Domain.Entities;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Lyvads.Domain.Repositories;

namespace Lyvads.Infrastructure.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly AppDbContext _context;

    public PromotionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Promotion promotion)
    {
        await _context.Promotions.AddAsync(promotion);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Promotion promotion)
    {
        _context.Promotions.Update(promotion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Promotion promotion)
    {
        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync();
    }

    public async Task<Promotion> GetByIdAsync(string id)
    {
        return await _context.Promotions.FindAsync(id) ?? new Promotion();
    }

    public async Task<List<Promotion>> GetAllAsync()
    {
        return await _context.Promotions.ToListAsync();
    }
}

