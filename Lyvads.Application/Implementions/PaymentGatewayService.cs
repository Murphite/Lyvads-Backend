using Stripe;
using Lyvads.Application.Dtos;
using Lyvads.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Lyvads.Application.Implementions;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly string _stripeSecretKey;
    private readonly StripeClient _stripeClient;

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

    public async Task<Result> ProcessPaymentAsync(decimal amount, string currency, string source, string description)
    {
        try
        {
            var options = new ChargeCreateOptions
            {
                Amount = (long)(amount * 100), // Amount in cents
                Currency = currency,
                Source = source,
                Description = description,
            };

            var service = new ChargeService(_stripeClient);
            var charge = await service.CreateAsync(options);

            if (charge.Status == "succeeded")
                return Result.Success();
            else
                return Result.Failure(new Error[] { new("Payment.Error", "Payment failed") });
        }
        catch (Exception ex)
        {
            // Log exception
            return Result.Failure(new Error[] { new("Payment.Error", ex.Message) });
        }
    }
}