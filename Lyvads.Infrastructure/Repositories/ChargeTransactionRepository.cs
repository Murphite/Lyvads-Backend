

using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class ChargeTransactionRepository : IChargeTransactionRepository
{
    private readonly AppDbContext _context;

    public ChargeTransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<ChargeTransaction> chargeTransactions)
    {
        if (chargeTransactions == null || !chargeTransactions.Any())
        {
            throw new ArgumentException("The charge transactions collection is null or empty.", nameof(chargeTransactions));
        }

        try
        {
            await _context.ChargeTransactions.AddRangeAsync(chargeTransactions);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log the exception (use a logging framework, e.g., Serilog, NLog)
            throw new InvalidOperationException("An error occurred while adding charge transactions.", ex);
        }
    }

    public async Task<List<ChargeTransaction>> GetAllAsync()
    {
        return await _context.ChargeTransactions.ToListAsync();
    }

    public async Task<List<Charge>> GetChargeDetailsAsync()
    {
        try
        {
            // Fetch charge details from the database
            return await _context.Charges.ToListAsync();
        }
        catch (Exception ex)
        {
            // Handle exceptions (logging, rethrowing, etc.)
            throw new Exception("Error fetching charge details from the database", ex);
        }
    }

    public async Task<ChargeTransaction> GetByCTIdAsync(string chargeId)
    {
        var chargeById =  await _context.ChargeTransactions.FindAsync(chargeId);

        if (chargeById == null)
            throw new Exception("Charge Transaction was not found");

        return chargeById;
    }

    public async Task AddAsync(Charge charge)
    {
        await _context.Charges.AddAsync(charge);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Charge charge)
    {
        _context.Charges.Update(charge);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Charge charge)
    {
        _context.Charges.Remove(charge);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Charge>> GetAllChargesAsync()
    {
        return await _context.Charges.ToListAsync();
    }

    public async Task<Charge> GetChargeByIdAsync(string chargeId)
    {
        var chargeById = await _context.Charges.FindAsync(chargeId);

        if (chargeById == null)
            throw new Exception("Charge was not found");

        return chargeById;
    }

    public async Task<ApplicationUser> GetApplicationUserWithRoles(string id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Id == id);

        if (user == null)
            return null!;

        //var roles = await _userManager.GetRolesAsync(user);
        //user.RoleConstants = roles.ToList();

        return user;
    }

}