
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Responses;
using Stripe.Checkout;

namespace Lyvads.Application.Interfaces;

public interface IPaymentGatewayService
{
    Task<ServerResponse<PaymentResponseDto>> InitializePaymentAsync(int amount, string email, string name);
    Task<ServerResponse<PaymentResponseDto>> InitializeRequestPaymentAsync(int amount, string email, string name);
    Task<ServerResponse<string>> VerifyPaymentAsync(string reference);
    bool VerifyPaystackSignature(string payload, string signature, string secretKey);
    Task StoreTransactionAsync(PaystackWebhookPayload payload);
    Task<Transaction> GetTransactionByReferenceAsync(string reference);
    Task UpdateTransactionAsync(Transaction transaction);

    Task<Result> Withdraw(string stripeAccountId, decimal amount, string currency);
    Task<Result> ProcessPaymentAsync(decimal amount, string currency, string source, string description);
    Task<Session> CreateCardPaymentSessionAsync(PaymentDTO payment, string domain);
    Task<Session> CreateOnlinePaymentSessionAsync(PaymentDTO payment, string domain);

}
