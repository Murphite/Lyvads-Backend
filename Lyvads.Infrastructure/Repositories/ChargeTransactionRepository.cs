

using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class ChargeTransactionRepository : IChargeTransactionRepository
{
    private readonly AppDbContext _context;

    public ChargeTransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChargeTransaction>> GetAllAsync()
    {
        return await _context.ChargeTransactions.ToListAsync();
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
}