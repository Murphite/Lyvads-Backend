

using Lyvads.Application.Dtos;

namespace Lyvads.Application.Interfaces;

public interface IWalletService
{    
    Task<Result> FundWalletViaCardAsync(string userId, decimal amount, string cardToken);
    //Task<Result> WithdrawFundsAsync(string userId, decimal amount);
    Task<decimal> GetBalanceAsync(string userId);
    Task<bool> DeductBalanceAsync(string userId, decimal amount);
}
