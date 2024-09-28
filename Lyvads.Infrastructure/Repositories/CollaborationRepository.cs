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
    

    public IQueryable<Collaboration> GetAllCollaborations()
    {
        return _context.Collaborations.AsQueryable(); 
    }

    public async Task<List<Collaboration>> GetAllAsync()
    {
        return await _context.Collaborations
            .Include(c => c.RegularUser)
            .Include(c => c.Creator)
            .ToListAsync();
    }

    public async Task<Collaboration?> GetByIdAsync(string id)
    {
        return await _context.Collaborations
            .Include(c => c.RegularUser)
            .Include(c => c.Creator)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

}

