using Lyvads.Application.Interfaces;
using Lyvads.Application.Dtos;
using Stripe;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Interfaces;
using Lyvads.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Lyvads.Application.Dtos.WalletDtos;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Mvc;
using Lyvads.Infrastructure.Repositories;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Lyvads.Application.Implementations;

public class WalletService : IWalletService
{
    private readonly IRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWalletRepository _walletRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<WalletService> _logger;
    private readonly IBankAccountRepository _bankrepository;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WalletService(IRepository repository,
        IUnitOfWork unitOfWork,
        IWalletRepository walletRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<WalletService> logger,
        IBankAccountRepository bankrepository,
        IPaymentGatewayService paymentGatewayService,
        IBankAccountRepository bankAccountRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _walletRepository = walletRepository;
        _userManager = userManager;
        _logger = logger;
        _bankrepository = bankrepository;
        _paymentGatewayService = paymentGatewayService;
        _bankAccountRepository = bankAccountRepository;
        _httpContextAccessor = httpContextAccessor;
    }


    public async Task<ServerResponse<PaymentResponseDto>> FundWalletAsync(int amount, string email, string name)
    {

        // Initialize payment via payment gateway service
        var response = await _paymentGatewayService.InitializePaymentAsync(amount, email, name);
        if (!response.IsSuccessful)
        {
            return new ServerResponse<PaymentResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = response.ResponseMessage
            };
        }

        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        if (user == null)
        {
            return new ServerResponse<PaymentResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found."
            };
        }

        var wallet = await _walletRepository.GetWalletWithTransactionsAsync(user.Id);
        if (wallet == null)
        {
            return new ServerResponse<PaymentResponseDto>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Wallet not found."
            };
        }


        // Create a new transaction for the wallet funding
        var transaction = new Transaction
        {
            ApplicationUserId = user.Id,
            Amount = amount,
            Email = user.Email,
            Name = user.FullName,
            TrxRef = response.Data.PaymentReference,
            WalletId = wallet.Id,
            Status = false, // Initially false, waiting for webhook confirmation
            Type = TransactionType.Funding,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };


        // Save transaction using the repository
        var savedTransaction = await _walletRepository.AddTransactionAsync(transaction);

        // Update the wallet balance
        //wallet.Balance += amount;
        //await _walletRepository.UpdateWalletAsync(wallet);

        var paymentResponseDto = new PaymentResponseDto
        {
            PaymentReference = response.Data.PaymentReference,
            AuthorizationUrl = response.Data.AuthorizationUrl,
            CancelUrl = response.Data.CancelUrl,
            UserName = user.FullName,
            UserEmail = user.Email,
            Amount = amount,
            WalletId = wallet.Id,
            DateCreated = transaction.CreatedAt
        };

        return new ServerResponse<PaymentResponseDto>
        {
            IsSuccessful = true,
            ResponseCode = "200",
            ResponseMessage = "Wallet funded successfully.",
            Data = paymentResponseDto
        };
    }

    public async Task<ServerResponse<List<WalletTrasactionResponseDto>>> GetWalletTransactions()
    {
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        if (user == null)
        {
            return new ServerResponse<List<WalletTrasactionResponseDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "User not found."
            };
        }

        var wallet = await _walletRepository.GetWalletWithTransactionsAsync(user.Id);
        if (wallet == null)
        {
            return new ServerResponse<List<WalletTrasactionResponseDto>>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Wallet not found."
            };
        }

        var transactions = wallet.Transactions.Select(t => new WalletTrasactionResponseDto
        {
            Id = t.Id,
            Amount = t.Amount,
            Name = t.Name,
            Email = t.Email,
            TransactionType = t.Type.ToString(),
            TrxRef = t.TrxRef,
            DateCreated = t.CreatedAt,
            Status = t.Status
        }).ToList();

        return new ServerResponse<List<WalletTrasactionResponseDto>>
        {
            IsSuccessful = true,
            ResponseCode = "200",
            ResponseMessage = "Transactions retrieved successfully.",
            Data = transactions
        };
    }


    public async Task<ServerResponse<string>> FundWalletViaCardAsync(string userId, decimal amount, 
        string paymentMethodId, string currency)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),  // Convert amount to cents
            Currency = currency.ToLower(),  // Set currency dynamically
            PaymentMethod = paymentMethodId,
            ConfirmationMethod = "manual",
            CaptureMethod = "automatic",
        };

        var service = new PaymentIntentService();
        try
        {
            PaymentIntent intent = await service.CreateAsync(options);

            return new ServerResponse<string>(true)
            {
                ResponseCode = "200",
                ResponseMessage = "Payment intent created successfully.",
                Data = intent.ClientSecret  // Return client_secret for frontend to complete the payment
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment failed.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "Payment processing error.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "PaymentError",
                    ResponseDescription = ex.Message
                }
            };
        }
    }


    public async Task<ServerResponse<string>> FundWalletViaOnlinePaymentAsync(string userId, decimal amount, string paymentMethodId, string currency)
    {
        // Create options for PaymentIntent
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),  // Convert amount to cents
            Currency = currency,
            PaymentMethod = paymentMethodId,  // PaymentMethodId from frontend
            ConfirmationMethod = "manual",   // Set to manual for frontend confirmation
            CaptureMethod = "automatic",
        };

        var service = new PaymentIntentService();
        try
        {
            // Create the payment intent via Stripe
            PaymentIntent intent = await service.CreateAsync(options);

            // Check if further action is required
            if (intent.Status == "requires_action" || intent.Status == "requires_confirmation")
            {
                return new ServerResponse<string>
                {
                    IsSuccessful = true,
                    ResponseCode = "200",
                    ResponseMessage = "Authentication required.",
                    Data = intent.ClientSecret  // Frontend will use this to complete payment authentication
                };
            }

            // Return success response if no further action is required
            return new ServerResponse<string>
            {
                IsSuccessful = true,
                ResponseCode = "200",
                ResponseMessage = "Payment intent created successfully.",
                Data = intent.ClientSecret  // Return the client secret for frontend
            };
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Online payment failed.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Failed to process online payment",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "StripeError",
                    ResponseDescription = stripeEx.Message
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Online payment failed.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "An error occurred while processing your payment.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "PaymentError",
                    ResponseDescription = ex.Message
                }
            };
        }
    }

    
    // This method will confirm the payment and credit the wallet if successful
    public async Task<ServerResponse<string>> ConfirmPaymentAsync(string paymentIntentId, string userId, decimal amount)
    {
        var service = new PaymentIntentService();
        try
        {
            PaymentIntent intent = await service.ConfirmAsync(paymentIntentId);

            if (intent.Status == "succeeded")
            {
                var walletResult = await CreditWalletAsync(userId, amount);
                if (walletResult.IsSuccess)
                {
                    return new ServerResponse<string>(true)
                    {
                        ResponseCode = "200",
                        ResponseMessage = "Payment confirmed and wallet funded successfully.",
                        Data = intent.Id
                    };
                }
                else
                {
                    return new ServerResponse<string>
                    {
                        IsSuccessful = false,
                        ResponseCode = "500",
                        ResponseMessage = "Failed to credit the wallet after payment confirmation.",
                        ErrorResponse = new ErrorResponse
                        {
                            ResponseCode = "500",
                            ResponseMessage = "Wallet funding error",
                            ResponseDescription = "Failed to credit the wallet after payment succeeded."
                        }
                    };
                }
            }
            else if (intent.Status == "requires_action")
            {
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ResponseCode = "402",
                    ResponseMessage = "Authentication required",
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = "402",
                        ResponseMessage = "requires_action",
                        ResponseDescription = "Additional authentication is still required to complete the payment."
                    }
                };
            }
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Payment confirmation failed.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Failed to confirm payment",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "400",
                    ResponseMessage = "StripeError",
                    ResponseDescription = stripeEx.Message
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment confirmation failed.");
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "An error occurred while confirming your payment.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "PaymentError",
                    ResponseDescription = ex.Message
                }
            };
        }

        return new ServerResponse<string>
        {
            IsSuccessful = false,
            ResponseCode = "400",
            ResponseMessage = "Failed to confirm payment.",
            ErrorResponse = new ErrorResponse
            {
                ResponseCode = "400",
                ResponseMessage = "UnknownError",
                ResponseDescription = "An unknown error occurred during the payment confirmation process."
            }
        };
    }

    public async Task<ServerResponse<string>> WithdrawToBankAccountAsync(string userId, decimal amount, string bankCardId)
    {
        // Check if the user has sufficient funds
        var walletBalance = await GetBalanceAsync(userId);
        if (walletBalance < amount)
        {
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "InsufficientFunds.Error",
                ResponseMessage = "Insufficient funds",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "InsufficientFunds.Error",
                    ResponseMessage = "You do not have enough funds in your wallet to complete this withdrawal."
                }
            };
        }

        // Create options for the withdrawal request (This assumes you have a payment processing service)
        var withdrawalOptions = new WithdrawalOptions
        {
            Amount = amount,
            Currency = "usd",
            BankCardId = bankCardId, // Assuming you have a way to identify the bank card
                                     // Add any other required options for the withdrawal
        };

        try
        {
            // Process the withdrawal (replace this with your actual payment processor logic)
            var withdrawalResult = await WithdrawAsync(withdrawalOptions);

            if (withdrawalResult.IsSuccess)
            {
                // If the withdrawal was successful, return success response
                return new ServerResponse<string>(true)
                {
                    ResponseCode = "00",
                    ResponseMessage = "Withdrawal to bank account successful.",
                    Data = withdrawalResult.TransactionId! // Assuming the processor returns a transaction ID
                };
            }
            else
            {
                // Handle failure in withdrawal processing
                return new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ResponseCode = withdrawalResult.ErrorCode!,
                    ResponseMessage = "Withdrawal failed",
                    ErrorResponse = new ErrorResponse
                    {
                        ResponseCode = withdrawalResult.ErrorCode,
                        ResponseMessage = withdrawalResult.ErrorMessage,
                        ResponseDescription = withdrawalResult.ErrorDescription // Assuming these fields exist in the withdrawal result
                    }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during withdrawal processing for User ID: {UserId}", userId);
            return new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = "An error occurred while processing the withdrawal.",
                ErrorResponse = new ErrorResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "WithdrawalError",
                    ResponseDescription = ex.Message
                }
            };
        }
    }


    public async Task<bool> DeductBalanceAsync(string userId, decimal amount)
    {
        // Retrieve the user's wallet
        var wallet = await _walletRepository.GetWalletAsync(userId);

        // Check if the wallet exists
        if (wallet == null)
        {
            throw new InvalidOperationException("Wallet not found.");
        }

        // Check if the user has enough balance
        if (wallet.Balance < amount)
        {
            throw new InvalidOperationException("Insufficient balance.");
        }

        // Deduct the amount from the wallet balance
        wallet.Balance -= amount;

        // Update the wallet in the database
        await _walletRepository.UpdateWalletAsync(wallet);

        // Return true if update was successful, otherwise false
        return true;
    }

    public async Task<decimal> GetBalanceAsync(string userId)
    {
        // Retrieve the user's wallet
        var wallet = await _walletRepository.GetWalletAsync(userId);

        // Check if the wallet exists
        if (wallet.Id == null)
        {
            _logger.LogError("Wallet not found for user {UserId}", userId);
            throw new InvalidOperationException("Wallet not found.");
        }         

        // Return the wallet balance
        return wallet.Balance;
    }

    private async Task<Result> CreditWalletAsync(string userId, decimal amount)
    {
        if (amount <= 0)
            return new Error[] { new("Amount.Error", "Amount must be greater than zero.") };

        try
        {
            // Retrieve the current balance
            var wallet = await _walletRepository.GetWalletAsync(userId);
            if (wallet == null)
                return new Error[] { new("Wallet.Error", "Wallet not found for the user.") };

            // Update the wallet balance
            wallet.Balance += amount;

            // Save the updated balance
            await _walletRepository.UpdateWalletAsync(wallet);

            // Optionally, log the operation
            _logger.LogInformation($"Credited {amount} to wallet of user {userId}.");

            return Result.Success("Credited amount to wallet of user was successful.");
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError(ex, "Error occurred while crediting wallet.");
            return new Error[] { new Error("Exception", ex.Message) };
        }
    }

    public async Task<WithdrawalResult> WithdrawAsync(WithdrawalOptions options)
    {
        try
        {
            // Create Stripe Transfer options
            var transferOptions = new TransferCreateOptions
            {
                Amount = (long)(options.Amount * 100), // Stripe requires amount in cents
                Currency = options.Currency, // For example "usd" or "ngn"
                Destination = options.BankCardId, // The Stripe account or card ID to transfer funds to
                TransferGroup = "LYVADS_WITHDRAWAL" // Optional: Grouping for easier tracking
            };

            var transferService = new TransferService();
            var transfer = await transferService.CreateAsync(transferOptions);

            // Return success if the transfer is completed
            return new WithdrawalResult
            {
                IsSuccess = true,
                TransactionId = transfer.Id,
                ErrorCode = null,
                ErrorMessage = null,
                ErrorDescription = null
            };
        }
        catch (StripeException stripeEx)
        {
            // Handle Stripe-specific exceptions
            return new WithdrawalResult
            {
                IsSuccess = false,
                ErrorCode = stripeEx.StripeError.Code,
                ErrorMessage = stripeEx.StripeError.Message,
                ErrorDescription = stripeEx.StripeError.DeclineCode
            };
        }
        catch (Exception ex)
        {
            // Handle general exceptions
            return new WithdrawalResult
            {
                IsSuccess = false,
                ErrorCode = "Payment.Error",
                ErrorMessage = ex.Message,
                ErrorDescription = ex.InnerException?.Message
            };
        }
    }


    public class WithdrawalOptions
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? BankCardId { get; set; } // The card ID or token used for withdrawal
    }

    public class WithdrawalResult
    {
        public bool IsSuccess { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDescription { get; set; }
    }


    //public async Task<Result> FundWalletViaBankTransferAsync(string userId, decimal amount)
    //{
    //    // Generate a unique reference for the transfer
    //    var transferReference = Guid.NewGuid().ToString();

    //    // Assuming a service API call to initiate a bank transfer
    //    var transferRequest = new BankTransferRequest
    //    {
    //        Amount = amount,
    //        Currency = "usd",
    //        UserId = userId,
    //        Reference = transferReference
    //    };

    //    var transferResponse = await _paymentGatewayService.InitiateBankTransferAsync(transferRequest);

    //    if (transferResponse.IsSuccess)
    //    {
    //        // Optionally, store transfer details in the database
    //        await _walletRepository.SaveTransferDetailsAsync(userId, amount, transferReference);

    //        return Result.Success(transferResponse.TransferInstructions);
    //    }

    //    return new Error[] { new("BankTransfer.Error", "Failed to initiate bank transfer.") };
    //}

    //public async Task<Result> WithdrawFundsAsync(string userId, decimal amount)
    //{
    //    if (amount <= 0)
    //        return new Error[] { new("Amount.Error", "Amount must be greater than zero.") };

    //    // Check if the user has sufficient balance
    //    var walletBalance = await _walletRepository.GetWalletBalanceAsync(userId);
    //    if (walletBalance < amount)
    //        return new Error[] { new("Wallet.Error", "Insufficient wallet balance.") };

    //    // Retrieve the bank account details for the user
    //    var bankAccount = await _bankAccountRepository.GetBankAccountByUserIdAsync(userId);
    //    if (bankAccount == null)
    //        return new Error[] { new("Amount.Error", "Bank account not found.") };

    //    // Create a BankTransferRequest
    //    var transferRequest = new BankTransferRequest
    //    {
    //        Amount = amount,
    //        Currency = "usd",
    //        UserId = userId,
    //        Reference = Guid.NewGuid().ToString(),
    //        BankName = bankAccount.BankName,
    //        AccountNumber = bankAccount.AccountNumber,
    //        AccountHolderName = bankAccount.AccountHolderName
    //    };

    //    // Initiate the bank transfer
    //    var transferResponse = await _paymentGatewayService.InitiateBankTransferAsync(transferRequest);

    //    if (transferResponse.IsSuccess)
    //    {
    //        // Deduct the balance from the wallet
    //        await DeductBalanceAsync(userId, amount);

    //        // Optionally, save the transaction details
    //        await _walletRepository.SaveWithdrawalDetailsAsync(userId, amount, transferResponse.TransferInstructions);

    //        return Result.Success("Withdrawal done successfully.");
    //    }

    //    return new Error[] { new("Withdrawal.Error", "Failed to process withdrawal.") };
    //}

    //public async Task<Result> FundWalletViaCardAsync(string userId, decimal amount, string cardToken)
    //{
    //    var options = new PaymentIntentCreateOptions
    //    {
    //        Amount = (long)(amount * 100), // Convert amount to cents
    //        Currency = "usd",
    //        PaymentMethod = cardToken,
    //        ConfirmationMethod = "automatic",
    //        Confirm = true,
    //    };

    //    var service = new PaymentIntentService();
    //    try
    //    {
    //        PaymentIntent intent = await service.CreateAsync(options);
    //        if (intent.Status == "succeeded")
    //        {
    //            // Credit the wallet
    //            var result = await CreditWalletAsync(userId, amount);
    //            if (result.IsSuccess)
    //                return Result.Success("Fund Wallet via Transfer was Successful.");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Card payment failed.");
    //        return new Error[] { new Error("Card payment failed:", ex.Message) };
    //    }

    //    return new Error[] { new("CardTransfer.Error", "Failed to process card payment.") };
    //}


}
