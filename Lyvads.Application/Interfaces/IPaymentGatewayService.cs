
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Stripe.Checkout;
using static Lyvads.Application.Implementions.PaymentGatewayService;

namespace Lyvads.Application.Interfaces;

public interface IPaymentGatewayService
{
    Task<Result> Withdraw(string stripeAccountId, decimal amount, string currency);
    Task<Result> ProcessPaymentAsync(decimal amount, string currency, string source, string description);
    Task<Session> CreateCardPaymentSessionAsync(PaymentDTO payment, string domain);
    Task<Session> CreateOnlinePaymentSessionAsync(PaymentDTO payment, string domain);

}
