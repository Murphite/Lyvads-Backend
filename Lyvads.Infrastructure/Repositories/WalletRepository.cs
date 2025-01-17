
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lyvads.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<WalletRepository> _logger;

    public WalletRepository(AppDbContext context,
        ILogger<WalletRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Wallet> GetWalletWithTransactionsAsync(string userId)
    {       
        var wallet = await _context.Wallets
            .Include(w => w.Transactions) 
            .FirstOrDefaultAsync(w => w.ApplicationUserId == userId);

        if (wallet == null)
        {
            throw new Exception($"Wallet not found for userId: {userId}");
        }

        return wallet;
    }


    public async Task<Wallet> GetByUserIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");

        var wallet = await _context.Wallets
                                   .FirstOrDefaultAsync(w => w.ApplicationUserId == userId);

        if (wallet == null)
        {
            throw new Exception("Wallet not found");
        }

        return wallet;
    }

    public async Task SaveTransferDetailsAsync(string userId, decimal amount, string transferReference)
    {
        // Retrieve the user based on userId
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            // Handle the case where the user is not found
            throw new ArgumentException("User not found", nameof(userId));
        }

        var transfer = new Transfer
        {
            UserId = userId,
            User = user, // Set the required User property
            Amount = amount,
            TransferReference = transferReference,
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Transfers.Add(transfer);
        await _context.SaveChangesAsync();
    }

    public void Update(Wallet wallet)
    {
        // Example: Entity Framework update implementation
        _context.Wallets.Update(wallet);
        _context.SaveChanges();
    }

    public async Task<Wallet?> GetByRequestIdAsync(string requestId)
    {
        if (string.IsNullOrEmpty(requestId))
        {
            throw new ArgumentException("Request ID cannot be null or empty.", nameof(requestId));
        }

        var request = await _context.Requests
            .Include(r => r.Wallet)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        return request?.Wallet;
    }



    public async Task<Transfer> GetTransferDetailsAsync(string transferReference)
    {
        var transfer = await _context.Transfers
                                     .FirstOrDefaultAsync(t => t.TransferReference == transferReference);

        if (transfer == null)
        {
            throw new Exception("Transfer not found");
        }

        return transfer;
    }


    public async Task UpdateTransferStatusAsync(Transfer transfer)
    {
        _context.Transfers.Update(transfer);
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetWalletBalanceAsync(string userId)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.ApplicationUserId == userId);

        return wallet?.Balance ?? 0;
    }

    public async Task SaveWithdrawalDetailsAsync(string userId, decimal amount, string transferReference)
    {
        var withdrawal = new Withdrawal
        {
            UserId = userId,
            Amount = amount,
            TransferReference = transferReference,
            Status = TransferStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Withdrawals.Add(withdrawal);
        await _context.SaveChangesAsync();
    }

    public async Task<Wallet> GetWalletAsync(string userId)
    {
        // Retrieve the user along with their wallet information
        var userWithWallet = await _context.Users
            .Where(u => u.Id == userId)
            .Include(u => u.Wallet) 
            .FirstOrDefaultAsync();

        // Check if the user was found
        if (userWithWallet == null)
            throw new InvalidOperationException("User not found.");

        // Check if the wallet exists
        if (userWithWallet.Wallet == null)
            throw new InvalidOperationException("Wallet not assigned to the user.");

        return userWithWallet.Wallet;
    }

    public async Task<Wallet> GetCreatorWalletAsync(string creatorId)
    {
        // Retrieve the creator along with their ApplicationUser and Wallet information
        var creatorWithWallet = await _context.Creators
            .Where(c => c.Id == creatorId)
            .Include(c => c.ApplicationUser)
            .ThenInclude(au => au.Wallet)
            .FirstOrDefaultAsync();

        // Check if the creator was found
        if (creatorWithWallet == null)
            throw new InvalidOperationException("Creator not found.");

        // Check if the ApplicationUser and their Wallet exist
        if (creatorWithWallet.ApplicationUser?.Wallet == null)
            throw new InvalidOperationException("Wallet not assigned to the creator.");

        return creatorWithWallet.ApplicationUser.Wallet;
    }



    public async Task AddAsync(Wallet wallet)
    {
        await _context.Wallets.AddAsync(wallet);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateWalletAsync(Wallet wallet)
    {
        _context.Wallets.Update(wallet);
        var result = await _context.SaveChangesAsync();
        return result > 0; 
    }

    public async Task<bool> SaveWalletChangesAsync(Wallet wallet)
    {
        _context.Wallets.Update(wallet);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Wallet?> GetWalletByUserIdAsync(string userId)
    {
        return await _context.Wallets
            .Include(w => w.ApplicationUser)
            .FirstOrDefaultAsync(w => w.ApplicationUserId == userId);
    }

    public async Task<Wallet?> GetWalletByCreatorIdAsync(string userId)
    {
        return await _context.Wallets
            .Include(w => w.ApplicationUser)
            .ThenInclude(a => a.Creator)
            .FirstOrDefaultAsync(w => w.ApplicationUser.Creator.Id == userId);
    }

    public async Task<Wallet?> GetWalletByRegularUserIdAsync(string userId)
    {
        return await _context.Wallets
            .Include(w => w.ApplicationUser)
            .ThenInclude(a => a.RegularUser)
            .FirstOrDefaultAsync(w => w.ApplicationUser.RegularUser.Id == userId);
    }

    public async Task<Transaction> AddTransactionAsync(Transaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task<Wallet> GetWalletByIdAsync(string walletId)
    {
        return await _context.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.Id == walletId);
    }

    public async Task<Transaction> GetTransactionByTrxRefAsync(string trxRef)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.TrxRef == trxRef);
    }


    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        _logger.LogInformation("Updating transaction with ID: {Id}, Status: {Status}", transaction.Id, transaction.Status);
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Transaction with ID: {Id} updated successfully.", transaction.Id);
    }


    public async Task<CardAuthorization> GetCardAuthorizationByEmailAsync(string email)
    {
        return await _context.CardAuthorizations
            .FirstOrDefaultAsync(ca => ca.Email == email);
    }

    public async Task StoreCardAuthorizationAsync(CardAuthorization cardAuthorization)
    {
        if (cardAuthorization == null)
            throw new ArgumentNullException(nameof(cardAuthorization));

        await _context.CardAuthorizations.AddAsync(cardAuthorization);
        await _context.SaveChangesAsync();
    }

}
