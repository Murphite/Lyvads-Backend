
using Lyvads.Application.Dtos;

namespace Lyvads.Application.Interfaces;

public interface IPaymentGatewayService
{
    Task<Result> Withdraw(string stripeAccountId, decimal amount, string currency);
}
