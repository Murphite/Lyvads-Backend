using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class CollaborationRepository : ICollaborationRepository
{
    private readonly AppDbContext _context;

    public CollaborationRepository(AppDbContext context)
    {
        _context = context;
    }
    

    public IQueryable<Request> GetAllCollaborations()
    {
        return _context.Requests.AsQueryable(); 
    }

    public async Task<List<Request>> GetAllAsync()
    {
        return await _context.Requests
            .Include(c => c.RegularUser!.ApplicationUser)
            .Include(c => c.Creator.ApplicationUser)
            .ToListAsync();
    }

    public async Task<Request?> GetByIdAsync(string id)
    {
        return await _context.Requests
            .Include(c => c.RegularUser!.ApplicationUser)
            .Include(c => c.Creator.ApplicationUser)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task UpdateAsync(Request entity)
    {
        _context.Requests.Update(entity);
        await _context.SaveChangesAsync();
    }

}

