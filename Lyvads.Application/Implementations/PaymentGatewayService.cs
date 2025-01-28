using Stripe;
using Lyvads.Application.Dtos;
using Lyvads.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Infrastructure.Persistence;
using PayStack.Net;
using Lyvads.Domain.Entities;
using Microsoft.Extensions.Logging;
using Lyvads.Domain.Responses;
using Microsoft.EntityFrameworkCore;
using Lyvads.Domain.Enums;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Lyvads.Domain.Repositories;

namespace Lyvads.Application.Implementations;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly StripeClient _stripeClient;
    private readonly string _stripeSecretKey;
    private readonly IConfiguration _configuration;
    private readonly string paystackToken;
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentGatewayService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWalletRepository _walletRepository;
    private readonly string _paystackSecretKey;
    private readonly HttpClient _httpClient;
    private PayStackApi Paystack { get; set; }

    public PaymentGatewayService(
        IConfiguration configuration, 
        AppDbContext context, 
        ILogger<PaymentGatewayService> logger,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
         HttpClient httpClient
        )
    {
        _configuration = configuration;
        paystackToken = _configuration["Paystack:PaystackSK"];
        Paystack = new PayStackApi(paystackToken);
        _context = context;
        _logger = logger;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _httpClient = httpClient;
        _stripeSecretKey = _configuration["Stripe:SecretKey"] ?? throw new ArgumentNullException(nameof(_stripeSecretKey));
        StripeConfiguration.ApiKey = _stripeSecretKey ?? throw new ArgumentNullException(nameof(StripeConfiguration.ApiKey));
        _stripeClient = new StripeClient(_stripeSecretKey);
    }

    public async Task<bool> RefundTransaction(Transaction transaction)
    {
        try
        {
            // Assuming RefundTransactionAsync expects the transaction reference as a string
            var response = await RefundTransactionAsync(transaction.TrxRef);

            if (response.Status)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding transaction {TrxRef}", transaction.TrxRef);
        }
        return false;
    }



    public async Task<ApiResponse<bool>> RefundTransactionAsync(string transactionReference)
    {
        try
        {
            var apiUrl = $"https://api.paystack.co/transaction/charge_refund";

            // Assuming you have an API client to handle the request
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_paystackSecretKey}");

            var content = new StringContent(JsonConvert.SerializeObject(new { transaction_reference = transactionReference }), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, content);

            // Check if the response was successful
            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool> { Status = true, Data = true }; // Indicating success
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during refund.");
        }

        return new ApiResponse<bool> { Status = false, Data = false }; // Indicating failure
    }


    //public async Task<RefundResponse> RefundTransactionAsync(string transactionReference)
    //{
    //    try
    //    {
    //        // Prepare the request body for refunding the transaction
    //        var refundRequest = new
    //        {
    //            transaction = transactionReference // Reference of the transaction to refund
    //        };

    //        // Set up the HTTP request message
    //        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.paystack.co/refund")
    //        {
    //            Content = new StringContent(JsonConvert.SerializeObject(refundRequest), Encoding.UTF8, "application/json")
    //        };

    //        // Add the authorization header with the Paystack secret key
    //        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _paystackSecretKey);

    //        // Send the request to Paystack API
    //        var response = await _httpClient.SendAsync(requestMessage);

    //        // If the request is successful
    //        if (response.IsSuccessStatusCode)
    //        {
    //            var content = await response.Content.ReadAsStringAsync();
    //            var refundResponse = JsonConvert.DeserializeObject<RefundResponse>(content);
    //            return refundResponse; // Return the successful refund response
    //        }

    //        // If the request fails, log the error and return a failure response
    //        var errorContent = await response.Content.ReadAsStringAsync();
    //        return new RefundResponse
    //        {
    //            Status = "failure",
    //            Message = $"Error: {errorContent}"
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        // Log the exception and return an error response
    //        return new RefundResponse
    //        {
    //            Status = "error",
    //            Message = $"An error occurred: {ex.Message}"
    //        };
    //    }
    //}

    public async Task<ServerResponse<PaymentResponseDto>> InitializePaymentAsync(int amount, string email, string name)
    {
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name) || amount <= 0)
        {
            return new ServerResponse<PaymentResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Invalid input. Amount, email, and name are required."
            };
        }

        var reference = Generate().ToString();

        var request = new TransactionInitializeRequest
        {
            AmountInKobo = amount * 100,
            Email = email,
            Reference = reference,
            Currency = "NGN",
            CallbackUrl = "https://radiksez.admin.lyvads.com/",
            Metadata = System.Text.Json.JsonSerializer.Serialize(new { CancelUrl = "https://cancel.com" })
        };

        var response = Paystack.Transactions.Initialize(request);
        if (response.Status)
        {
            // Check if the transaction already exists
            var existingTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TrxRef == reference);

            if (existingTransaction == null)
            {
                var transaction = new Transaction
                {
                    ApplicationUserId = user.Id,
                    Amount = amount,
                    Email = email,
                    TrxRef = reference,
                    Name = name,
                    Status = false // Set to false initially; webhook will confirm
                };

                //await _context.Transactions.AddAsync(transaction);
                //await _context.SaveChangesAsync();
            }

            return new ServerResponse<PaymentResponseDto>
            {
                IsSuccessful = true,
                ResponseCode = "200",
                ResponseMessage = "Payment initialized successfully.",
                Data = new PaymentResponseDto
                {
                    PaymentReference = reference,
                    AuthorizationUrl = response.Data.AuthorizationUrl,
                    CancelUrl = "https://cancel.com"
                }
            };
        }

        _logger.LogError($"Payment initialization failed: {response.Message}");
        return new ServerResponse<PaymentResponseDto>
        {
            IsSuccessful = false,
            ResponseCode = "500",
            ResponseMessage = response.Message
        };
    }

    public async Task<ServerResponse<PaymentResponseDto>> InitializeRequestPaymentAsync(int amount, string email, string name)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name) || amount <= 0)
        {
            return new ServerResponse<PaymentResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Invalid input. Amount, email, and name are required."
            };
        }

        var request = new TransactionInitializeRequest
        {
            AmountInKobo = amount * 100,
            Email = email,
            Reference = Generate().ToString(), 
            Currency = "NGN",
            CallbackUrl = "https://radiksez.admin.lyvads.com",

        };

        var response = Paystack.Transactions.Initialize(request);
        if (response.Status)
        {
            // Check if the transaction already exists
            var existingTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TrxRef == request.Reference);

            if (existingTransaction == null)
            {
                // Create the new transaction object
                var transaction = new Transaction
                {
                    Amount = amount,
                    Email = email,
                    TrxRef = request.Reference,
                    Name = name,
                    Status = false 
                };

                //// Add the new transaction to the database
                //await _context.Transactions.AddAsync(transaction);
                //await _context.SaveChangesAsync();
            }

            return new ServerResponse<PaymentResponseDto>
            {
                IsSuccessful = true,
                ResponseCode = "200",
                ResponseMessage = "Payment initialized successfully.",
                Data = new PaymentResponseDto
                {
                    PaymentReference = request.Reference,
                    AuthorizationUrl = response.Data.AuthorizationUrl
                }
            };
        }

        _logger.LogError($"Payment initialization failed: {response.Message}");
        return new ServerResponse<PaymentResponseDto>
        {
            IsSuccessful = false,
            ResponseCode = "500",
            ResponseMessage = response.Message
        };
    }


    public async Task<ServerResponse<string>> VerifyPaymentAsync(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Transaction reference is required."
            };
        }

        var response = Paystack.Transactions.Verify(reference);
        if (response.Data.Status == "success")
        {
            var transaction = await _context.Transactions
                .Where(x => x.TrxRef == reference)
                .FirstOrDefaultAsync();

            if (transaction != null)
            {
                transaction.Status = true;
                _context.Transactions.Update(transaction);
                await _context.SaveChangesAsync();

                return new ServerResponse<string>
                {
                    IsSuccessful = true,
                    ResponseCode = "200",
                    ResponseMessage = "Payment verified successfully."
                };
            }

            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Transaction not found."
            };
        }

        return new ServerResponse<string>
        {
            IsSuccessful = false,
            ResponseCode = "400",
            ResponseMessage = response.Data.GatewayResponse
        };
    }

    private static int Generate()
    {
        Random rand = new Random((int)DateTime.Now.Ticks);
        return rand.Next(100000000, 999999999);
    }

    public bool VerifyPaystackSignature(string payload, string signature, string secretKey)
    {
        // Serialize the payload to a JSON string (make sure it matches the request body exactly)
        var payloadString = JsonConvert.SerializeObject(payload);

        // Compute the HMAC-SHA256 hash using the secretKey and payload string
        var computedHash = ComputeHmacSha256(payloadString, secretKey);

        // Compare the computed hash with the signature provided in the webhook header
        return computedHash == signature;
    }

    private string ComputeHmacSha256(string data, string key)
    {
        // Convert data and key into byte arrays
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        // Create a new HMACSHA256 instance with the provided secret key
        using (var hmacsha256 = new HMACSHA256(keyBytes))
        {
            // Compute the hash
            var hashBytes = hmacsha256.ComputeHash(dataBytes);

            // Convert the hash to a hexadecimal string and return
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public async Task<Transaction> GetTransactionByReferenceAsync(string reference)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.TrxRef == reference);
    }

    // Update transaction
    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
    }


    private string ComputeSha256Hash(string data, string key)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public async Task StoreTransactionAsync(PaystackWebhookPayload payload)
    {
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        // Log the received data for debugging
        _logger.LogInformation("Received transaction reference: {Reference}, Status: {Status}",
                                payload.Data.Reference, payload.Data.Status);

        // Check if the transaction already exists
        var existingTransaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.TrxRef == payload.Data.Reference);

        if (existingTransaction != null)
        {
            // Log the current status before updating
            _logger.LogInformation("Current status of transaction {Reference}: {Status}",
                                    existingTransaction.TrxRef, existingTransaction.Status);

            // Only update if status is different
            if (existingTransaction.Status != (payload.Data.Status == "success"))
            {
                existingTransaction.Status = payload.Data.Status == "success";  // Map status to bool
                existingTransaction.UpdatedAt = DateTimeOffset.Now;
                _context.Transactions.Update(existingTransaction);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Transaction with reference {Reference} updated to status {Status}.",
                                        payload.Data.Reference, existingTransaction.Status);
            }
            else
            {
                _logger.LogInformation("Transaction {Reference} is already in status {Status}, no update required.",
                                        payload.Data.Reference, existingTransaction.Status);
            }
        }
        else
        {
            // Create a new transaction if it doesn't exist
            var transaction = new Transaction
            {
                ApplicationUserId = user.Id,
                Amount = payload.Data.Amount / 100,
                Email = payload.Data.Email,
                TrxRef = payload.Data.Reference,
                Status = payload.Data.Status == "success", // Map status to bool
                Type = TransactionType.Funding,
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now
            };

            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
            _logger.LogInformation("New transaction created with reference {Reference}.", payload.Data.Reference);
        }
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