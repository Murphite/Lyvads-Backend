

using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Transaction> GetAllPayments()
    {
        // Filter the transactions by TransactionType.Payment
        return _context.Set<Transaction>().Where(t => t.Type == TransactionType.Payment);
    }

    public async Task<Transaction?> GetByIdAsync(string id)
    {
        return await _context.Transactions
            .Include(t => t.Request) 
            .Include(t => t.ApplicationUser)
            .FirstOrDefaultAsync(t => t.RequestId == id);
    }

    public async Task<bool> CreateTransactionAsync(Transaction transaction)
    {
        // Add the transaction to the DbSet for transactions
        await _context.Transactions.AddAsync(transaction);

        // Save the changes asynchronously to the database
        var result = await _context.SaveChangesAsync();

        // If SaveChangesAsync returns a number greater than 0, the transaction was successfully added
        return result > 0;

    }

}
