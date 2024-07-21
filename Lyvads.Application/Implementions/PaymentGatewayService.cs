using Stripe;
using Lyvads.Application.Dtos;
using Lyvads.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Lyvads.Application.Implementions;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly string _stripeSecretKey;

    public PaymentGatewayService(IConfiguration configuration)
    {
        _stripeSecretKey = configuration["Stripe:SecretKey"];
        StripeConfiguration.ApiKey = _stripeSecretKey;
    }

    public async Task<Result> Withdraw(string stripeAccountId, decimal amount, string currency)
    {
        try
        {
            var options = new TransferCreateOptions
            {
                Amount = (long)(amount * 100), // Stripe requires amount in the smallest currency unit
                Currency = currency, // Use appropriate currency (e.g., "usd" or "ngn")
                Destination = stripeAccountId,
                TransferGroup = "ORDER_95", // Optional
            };

            var service = new TransferService();
            var transfer = await service.CreateAsync(options);

            // Check transfer status, etc.
            return Result.Success();
        }
        catch (Exception ex)
        {
            // Log and handle the exception
            return new Error[] { new ("Payment.Error", ex.Message) };
        }
    }
}