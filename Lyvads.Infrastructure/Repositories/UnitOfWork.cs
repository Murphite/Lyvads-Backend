using Lyvads.Infrastructure.Persistence;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync()
    {
        UpdateAuditableEntities();
        return await _context.SaveChangesAsync();
    }

    private void UpdateAuditableEntities()
    {
        var entities = _context.ChangeTracker.Entries<IAuditable>();

        foreach (var entry in entities)
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(e => e.CreatedAt).CurrentValue = DateTimeOffset.UtcNow;
                    entry.Property(e => e.CreatedAt).CurrentValue = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Property(e => e.UpdatedAt).CurrentValue = DateTimeOffset.UtcNow;
                    break;
            }
    }
}
