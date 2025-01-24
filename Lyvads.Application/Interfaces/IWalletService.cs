

using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lyvads.Application.Interfaces;

public interface IWalletService
{
    Task<ServerResponse<StorePaymentCardResponseDto>> StoreCardForRecurringPayment(StoreCardRequest request);
    Task<ServerResponse<List<WalletTrasactionResponseDto>>> GetWalletTransactions();
    Task<ServerResponse<PaymentResponseDto>> FundWalletAsync(int amount, string email, string name);
    Task<ServerResponse<string>> FundWalletViaCardAsync(string userId, decimal amount, string paymentMethodId, string currency);
    Task<ServerResponse<string>> ConfirmPaymentAsync(string paymentIntentId, string userId, decimal amount);
    Task<ServerResponse<string>> FundWalletViaOnlinePaymentAsync(string userId, decimal amount, string paymentMethodId, string currency);
    Task<ServerResponse<string>> WithdrawToBankAccountAsync(string userId, decimal amount, string bankCardId);
    Task<decimal> GetBalanceAsync(string userId);
    Task<bool> DeductBalanceAsync(string userId, decimal amount);
    Task CreditWalletAmountAsync(string walletId, decimal amount);
}
