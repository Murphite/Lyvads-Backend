
using Lyvads.Application.Dtos;

namespace Lyvads.Application.Interfaces;

public interface IPaymentGatewayService
{
    Task<Result> Withdraw(string stripeAccountId, decimal amount, string currency);
    Task<Result> ProcessPaymentAsync(decimal amount, string currency, string source, string description);
}
