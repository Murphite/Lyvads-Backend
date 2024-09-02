

using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IBankAccountRepository
{
    Task<BankAccount> GetBankAccountByUserIdAsync(string userId);
}
