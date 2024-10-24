

using Lyvads.Application.Dtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Interfaces;

public interface IWalletService
{
    Task<ServerResponse<string>> FundWalletViaCardAsync(string userId, decimal amount, string paymentMethodId, string currency);
    Task<ServerResponse<string>> ConfirmPaymentAsync(string paymentIntentId, string userId, decimal amount);
    Task<ServerResponse<string>> FundWalletViaOnlinePaymentAsync(string userId, decimal amount, string paymentMethodId, string currency);
    Task<ServerResponse<string>> WithdrawToBankAccountAsync(string userId, decimal amount, string bankCardId);
    Task<decimal> GetBalanceAsync(string userId);
    Task<bool> DeductBalanceAsync(string userId, decimal amount);
}
