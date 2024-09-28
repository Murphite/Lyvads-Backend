using Lyvads.Application.Interfaces;
using Lyvads.Application.Dtos;
using Stripe;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Interfaces;
using Lyvads.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Lyvads.Application.Dtos.WalletDtos;

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

    public WalletService(IRepository repository,
        IUnitOfWork unitOfWork,
        IWalletRepository walletRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<WalletService> logger,
        IBankAccountRepository bankrepository,
        IPaymentGatewayService paymentGatewayService,
        IBankAccountRepository bankAccountRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _walletRepository = walletRepository;
        _userManager = userManager;
        _logger = logger;
        _bankrepository = bankrepository;
        _paymentGatewayService = paymentGatewayService;
        _bankAccountRepository = bankAccountRepository;
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

    public async Task<Result> FundWalletViaOnlineAsync(string userId, decimal amount, string paymentMethodId)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Convert amount to the smallest currency unit (cents for USD)
            Currency = "usd", // Specify the currency
            PaymentMethod = paymentMethodId, // The payment method ID provided by Stripe (e.g., for bank transfer, wallet, etc.)
            ConfirmationMethod = "automatic", // Automatically confirm the payment
            Confirm = true, // Automatically confirm the PaymentIntent after creation
        };

        var service = new PaymentIntentService();
        try
        {
            // Create the PaymentIntent
            PaymentIntent intent = await service.CreateAsync(options);

            // Check if the payment succeeded
            if (intent.Status == "succeeded")
            {
                // Credit the wallet
                var result = await CreditWalletAsync(userId, amount);
                if (result.IsSuccess)
                    return Result.Success("Fund Wallet via Online Payment was Successful.");
            }
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Online payment failed.");
            return new Error[] { new Error("Online payment failed:", ex.Message) };

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during online payment.");
            return new Error[] { new Error("Unexpected error occurred:", ex.Message) };

        }

        return new Error[] { new Error("CardTransfer.Error", "Failed to process online payment.") };
    }

    public async Task<Result> FundWalletViaCardAsync(string userId, decimal amount, string cardToken)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Convert amount to cents
            Currency = "usd",
            PaymentMethod = cardToken,
            ConfirmationMethod = "automatic",
            Confirm = true,
        };

        var service = new PaymentIntentService();
        try
        {
            PaymentIntent intent = await service.CreateAsync(options);
            if (intent.Status == "succeeded")
            {
                // Credit the wallet
                var result = await CreditWalletAsync(userId, amount);
                if (result.IsSuccess)
                    return Result.Success("Fund Wallet via Transfer was Successful.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Card payment failed.");
            return new Error[] { new Error("Card payment failed:", ex.Message) };
        }

        return new Error[] { new("CardTransfer.Error", "Failed to process card payment.") };
    }

    public async Task<Result> ConfirmBankTransferAsync(string transferReference)
    {
        var transfer = await _walletRepository.GetTransferDetailsAsync(transferReference);

        if (transfer != null && transfer.Status == "PENDING")
        {
            transfer.Status = "COMPLETED";
            await _walletRepository.UpdateTransferStatusAsync(transfer);

            // Credit the wallet
            var result = await CreditWalletAsync(transfer.UserId, transfer.Amount);
            if (result.IsSuccess)
            {
                return Result.Success("Confirmed Bank Transfer");
            }
        }
        return new Error[] { new("BankTransfer.Error", "Transfer confirmation failed.") };
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

}
