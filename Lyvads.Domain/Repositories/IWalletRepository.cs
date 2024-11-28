

using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Repositories;

public interface IWalletRepository
{
    Task<Wallet> GetWalletWithTransactionsAsync(string userId);
    Task SaveTransferDetailsAsync(string userId, decimal amount, string transferReference);
    Task<Transfer> GetTransferDetailsAsync(string transferReference);
    Task UpdateTransferStatusAsync(Transfer transfer);
    Task<decimal> GetWalletBalanceAsync(string userId);
    Task SaveWithdrawalDetailsAsync(string userId, decimal amount, string transferReference);
    Task<Wallet> GetWalletAsync(string userId);
    Task AddAsync(Wallet wallet);
    Task<bool> UpdateWalletAsync(Wallet wallet);
    Task<Wallet> GetByUserIdAsync(string userId);
    Task<Wallet?> GetWalletByUserIdAsync(string userId);
    Task<bool> SaveWalletChangesAsync(Wallet wallet);
    Task<Transaction> AddTransactionAsync(Transaction transaction);
    Task<Transaction> GetTransactionByTrxRefAsync(string trxRef); 
    Task UpdateTransactionAsync(Transaction transaction);
    Task<Wallet> GetWalletByIdAsync(string walletId);
    Task<Wallet> GetByRequestIdAsync(string requestId);
}
