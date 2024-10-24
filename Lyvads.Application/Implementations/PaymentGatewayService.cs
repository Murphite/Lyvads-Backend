using Stripe;
using Lyvads.Application.Dtos;
using Lyvads.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;
using Lyvads.Application.Dtos.RegularUserDtos;
using static Lyvads.Application.Implementations.WalletService;

namespace Lyvads.Application.Implementations;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly StripeClient _stripeClient;
    private readonly string _stripeSecretKey;
    private readonly IConfiguration _configuration;

    public PaymentGatewayService(IConfiguration configuration)
    {
        _stripeSecretKey = configuration["Stripe:SecretKey"] ?? throw new ArgumentNullException(nameof(_stripeSecretKey));
        StripeConfiguration.ApiKey = _stripeSecretKey ?? throw new ArgumentNullException(nameof(StripeConfiguration.ApiKey));

        // Initialize _stripeClient using the _stripeSecretKey
        _stripeClient = new StripeClient(_stripeSecretKey);
        _configuration = configuration;
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
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentException("Source cannot be null or empty", nameof(source));
            }

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

    public async Task<Session> CreateCardPaymentSessionAsync(PaymentDTO payment, string domain)
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = payment.Amount * 100, 
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = payment.ProductName
                        }
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = domain + "/success-payment?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = domain + payment.ReturnUrl
        };

        var service = new SessionService();
        return await service.CreateAsync(options);
    }

    public async Task<Session> CreateOnlinePaymentSessionAsync(PaymentDTO payment, string domain)
    {
        // This method could handle other online payments, using the same Stripe logic as for cards
        return await CreateCardPaymentSessionAsync(payment, domain);
    }
}