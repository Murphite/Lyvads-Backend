

using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IWalletRepository
{
    Task SaveTransferDetailsAsync(string userId, decimal amount, string transferReference);
    Task<Transfer> GetTransferDetailsAsync(string transferReference);
    Task UpdateTransferStatusAsync(Transfer transfer);
    Task<decimal> GetWalletBalanceAsync(string userId);
    Task SaveWithdrawalDetailsAsync(string userId, decimal amount, string transferReference);
    Task<Wallet> GetWalletAsync(string userId);
    Task AddAsync(Wallet wallet);
    Task<bool> UpdateWalletAsync(Wallet wallet);
    Task<Wallet> GetByUserIdAsync(string userId);
}
