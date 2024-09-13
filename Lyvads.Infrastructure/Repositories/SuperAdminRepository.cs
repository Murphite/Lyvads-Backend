using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace Lyvads.Infrastructure.Repositories;

public class SuperAdminRepository : ISuperAdminRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<SuperAdminRepository> _logger;

    public SuperAdminRepository(AppDbContext context, ILogger<SuperAdminRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(SuperAdmin superAdmin)
    {
        try
        {
            // Add the SuperAdmin entity to the context
            await _context.SuperAdmins.AddAsync(superAdmin);
            // Save changes to the database
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding the SuperAdmin.");
            throw; // Optionally rethrow or handle the exception as needed
        }
    }
}
