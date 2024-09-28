

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

}
