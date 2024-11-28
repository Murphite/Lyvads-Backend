

using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface ITransactionRepository
{
    IQueryable<Transaction> GetAllPayments();
    Task<bool> CreateTransactionAsync(Transaction transaction);
    Task<Transaction?> GetByIdAsync(string id);
}
