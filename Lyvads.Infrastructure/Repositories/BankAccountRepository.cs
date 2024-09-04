using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Repositories;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly AppDbContext _context;

    public BankAccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccount> GetBankAccountByUserIdAsync(string userId)
    {
        var bankAccount = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (bankAccount == null)
        {
            throw new InvalidOperationException($"No bank account found for user with ID {userId}");
        }

        return bankAccount;
    }

}
