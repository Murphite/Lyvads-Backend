﻿using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace Lyvads.API.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PaymentController : Controller
{
    private readonly IWalletService _walletService;
    private readonly IPaymentGatewayService _paymentService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PaymentController> _logger;
    private readonly string _paystackSecretKey;
    private readonly IConfiguration _configuration;
    private readonly IWalletRepository _walletRepository;
    private readonly IPromotionSubRepository _promotionSubRepository;
    private readonly IRequestRepository _requestRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUserService _currentUserService;

    public PaymentController(
        IConfiguration configuration,
        IWalletService walletService,
        IWalletRepository walletRepository,
        IPromotionSubRepository promotionSubRepository,
        IRequestRepository requestRepository,
        UserManager<ApplicationUser> userManager,
        IPaymentGatewayService paymentService,
        ILogger<PaymentController> logger,
        IHttpContextAccessor httpContextAccessor,
        ICurrentUserService currentUserService)
    {
        _configuration = configuration;
        _walletService = walletService;
        _userManager = userManager;
        _paymentService = paymentService;
        _logger = logger;
        _paystackSecretKey = _configuration["Paystack:PaystackSK"];
        _promotionSubRepository = promotionSubRepository;
        _walletRepository = walletRepository;
        _requestRepository = requestRepository;
        _httpContextAccessor = httpContextAccessor;
        _currentUserService = currentUserService;
    }

    [HttpPost("fund-wallet")]
    public async Task<IActionResult> FundWallet(int amount)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        if (amount <= 0 || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.FullName))
        {
            return BadRequest(new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Invalid input."
            });
        }

        var response = await _walletService.FundWalletAsync(amount, user.Email, user.FullName);
        if (!response.IsSuccessful)
            return BadRequest(response);

        return Ok(response);
    }


    [HttpGet("verify")]
    public async Task<IActionResult> VerifyPayment(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return BadRequest(new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Reference is required."
            });
        }

        var response = await _paymentService.VerifyPaymentAsync(reference);
        if (!response.IsSuccessful)
            return BadRequest(response);

        return Ok(response);
    }


    [HttpGet("wallet-transactions")]
    public async Task<IActionResult> GetWalletTransactions()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return BadRequest(new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "User not logged In."
            });
        }

        var response = await _walletService.GetWalletTransactions();
        if (!response.IsSuccessful)
            return BadRequest(response);

        return Ok(response);

    }


    //[HttpPost("paystack/webhook")]
    //[AllowAnonymous]
    //public async Task<IActionResult> PaystackWebhook()
    //{
    //    HttpContext.Request.EnableBuffering();
    //    string rawBody;

    //    using (var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
    //    {
    //        rawBody = await reader.ReadToEndAsync();
    //    }

    //    _logger.LogInformation("Webhook received with raw body: {RawBody}", rawBody);
    //    HttpContext.Request.Body.Position = 0;

    //    PaystackWebhookPayload payload;
    //    try
    //    {
    //        payload = JsonConvert.DeserializeObject<PaystackWebhookPayload>(rawBody);
    //        if (payload?.Data == null)
    //        {
    //            _logger.LogError("Invalid webhook payload format.");
    //            return BadRequest(new { status = "invalid_payload" });
    //        }

    //        _logger.LogInformation("Deserialized payload: {Payload}", JsonConvert.SerializeObject(payload));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to deserialize webhook payload.");
    //        return BadRequest(new { status = "invalid_payload_format" });
    //    }

    //    string trxRef = payload.Data.Reference;
    //    string status = payload.Data.Status;
    //    string email = payload.Data.Email;
    //    string authorizationCode = payload.Data.AuthorizationCode;

    //    if (string.IsNullOrEmpty(trxRef) || string.IsNullOrEmpty(status))
    //    {
    //        _logger.LogWarning("Transaction reference or status is missing.");
    //        return BadRequest(new { status = "missing_data" });
    //    }

    //    var transaction = await _walletRepository.GetTransactionByTrxRefAsync(trxRef);
    //    if (transaction == null)
    //    {
    //        _logger.LogWarning("Transaction with reference {TrxRef} not found.", trxRef);
    //        return BadRequest(new { status = "transaction_not_found" });
    //    }

    //    if (transaction.Status)
    //    {
    //        _logger.LogInformation("Transaction with reference {TrxRef} already processed.", trxRef);
    //        return Ok(new { status = "already_processed" });
    //    }

    //    if (status == "success")
    //    {
    //        transaction.Status = true;

    //        // Check if the transaction is for a request to a creator
    //        if (transaction.RequestId != null)
    //        {
    //            var request = await _requestRepository.GetRequestByIdAsync(transaction.RequestId);
    //            if (request != null && request.CreatorId != null)
    //            {
    //                decimal baseAmount = request.RequestAmount;
    //                decimal fastTrackFee = request.FastTrackFee;

    //                // Credit the creator's wallet with the base amount and fast track fee
    //                await _walletService.CreditWalletAmountAsync(request.CreatorId, baseAmount + fastTrackFee);
    //                _logger.LogInformation("Credited {Amount} to Creator ID: {CreatorId}", baseAmount + fastTrackFee, request.CreatorId);
    //            }
    //        }

    //        if (transaction.WalletId != null)
    //        {
    //            var wallet = await _walletRepository.GetWalletByIdAsync(transaction.WalletId);
    //            if (wallet != null)
    //            {
    //                wallet.Balance += transaction.Amount;
    //                await _walletRepository.UpdateWalletAsync(wallet);
    //                _logger.LogInformation("Wallet balance updated for WalletId: {WalletId}.", transaction.WalletId);
    //            }
    //        }

    //        // Store the card details after a successful payment
    //        var storeCardRequest = new StoreCardRequest
    //        {
    //            AuthorizationCode = authorizationCode,
    //            Email = email,
    //            CardType = payload.Data.CardType,
    //            Last4 = payload.Data.Last4,
    //            ExpiryMonth = payload.Data.ExpiryMonth,
    //            ExpiryYear = payload.Data.ExpiryYear,
    //            Bank = payload.Data.Bank,
    //            AccountName = payload.Data.AccountName,
    //            Reusable = payload.Data.Reusable,
    //            CountryCode = payload.Data.CountryCode,
    //            Bin = payload.Data.Bin,
    //            Signature = payload.Data.Signature,
    //            Channel = payload.Data.Channel
    //        };

    //        await _walletService.StoreCardForRecurringPayment(storeCardRequest);

    //        await _walletRepository.UpdateTransactionAsync(transaction);
    //        _logger.LogInformation("Transaction with reference {TrxRef} marked as successful.", trxRef);
    //        return Ok(new { status = "success" });
    //    }

    //    transaction.Status = false;
    //    await _walletRepository.UpdateTransactionAsync(transaction);
    //    _logger.LogInformation("Transaction with reference {TrxRef} marked as failed.", trxRef);
    //    return Ok(new { status = "failure" });
    //}

    [HttpPost("paystack/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PaystackWebhook()
    {
        HttpContext.Request.EnableBuffering();
        string rawBody;

        using (var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            rawBody = await reader.ReadToEndAsync();
        }

        _logger.LogInformation("Webhook received with raw body: {RawBody}", rawBody);
        HttpContext.Request.Body.Position = 0;

        PaystackWebhookPayload payload;
        try
        {
            payload = JsonConvert.DeserializeObject<PaystackWebhookPayload>(rawBody);
            if (payload?.Data == null)
            {
                _logger.LogError("Invalid webhook payload format.");
                return BadRequest(new { status = "invalid_payload" });
            }

            _logger.LogInformation("Deserialized payload: {Payload}", JsonConvert.SerializeObject(payload));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize webhook payload.");
            return BadRequest(new { status = "invalid_payload_format" });
        }

        string trxRef = payload.Data.Reference;
        string status = payload.Data.Status;

        if (string.IsNullOrEmpty(trxRef) || string.IsNullOrEmpty(status))
        {
            _logger.LogWarning("Transaction reference or status is missing.");
            return BadRequest(new { status = "missing_data" });
        }

        var transaction = await _walletRepository.GetTransactionByTrxRefAsync(trxRef);
        if (transaction == null)
        {
            _logger.LogWarning("Transaction with reference {TrxRef} not found.", trxRef);
            return BadRequest(new { status = "transaction_not_found" });
        }

        if (transaction.Status)
        {
            _logger.LogInformation("Transaction with reference {TrxRef} already processed.", trxRef);
            return Ok(new { status = "already_processed" });
        }

        if (status == "success")
        {
            transaction.Status = true;

            // Save the card details
            var storeCardRequest = new StoreCardRequest
            {
                AuthorizationCode = payload.Data.AuthorizationCode,
                Email = payload.Data.Email,
                CardType = payload.Data.CardType,
                Last4 = payload.Data.Last4,
                ExpiryMonth = payload.Data.ExpiryMonth,
                ExpiryYear = payload.Data.ExpiryYear,
                Bank = payload.Data.Bank,
                AccountName = payload.Data.AccountName,
                Reusable = payload.Data.Reusable,
                CountryCode = payload.Data.CountryCode,
                Bin = payload.Data.Bin,
                Signature = payload.Data.Signature,
                Channel = payload.Data.Channel
            };

            await _walletService.StoreCardForRecurringPayment(storeCardRequest);

            // Refund ₦50 for card saving
            if (transaction.Amount == 50)
            {
                var refundResult = await _paymentService.RefundTransaction(transaction);
                if (!refundResult)
                {
                    _logger.LogWarning("Refund of ₦50 failed for transaction: {TrxRef}", trxRef);
                }
                else
                {
                    _logger.LogInformation("Refund of ₦50 successfully processed for transaction: {TrxRef}", trxRef);
                }
            }

            await _walletRepository.UpdateTransactionAsync(transaction);
            _logger.LogInformation("Transaction with reference {TrxRef} marked as successful.", trxRef);

            // Fetch the associated subscription and mark it as active
            var subscription = await _promotionSubRepository.GetByPaymentReferenceAsync(trxRef);
            if (subscription != null)
            {
                subscription.IsActive = true;
                await _promotionSubRepository.UpdateAsync(subscription);
                _logger.LogInformation("Subscription for creator {CreatorId} marked as active.", subscription.CreatorId);
            }


            return Ok(new { status = "success" });
        }

        transaction.Status = false;
        await _walletRepository.UpdateTransactionAsync(transaction);
        _logger.LogInformation("Transaction with reference {TrxRef} marked as failed.", trxRef);
        return Ok(new { status = "failure" });
    }


    //[HttpPost("paystack/webhook")]
    //[AllowAnonymous]
    //public async Task<IActionResult> PaystackWebhook()
    //{
    //    HttpContext.Request.EnableBuffering();
    //    string rawBody;

    //    using (var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
    //    {
    //        rawBody = await reader.ReadToEndAsync();
    //    }

    //    _logger.LogInformation("Webhook received with raw body: {RawBody}", rawBody);
    //    HttpContext.Request.Body.Position = 0;

    //    PaystackWebhookPayload payload;
    //    try
    //    {
    //        payload = JsonConvert.DeserializeObject<PaystackWebhookPayload>(rawBody);
    //        if (payload?.Data == null)
    //        {
    //            _logger.LogError("Invalid webhook payload format.");
    //            return BadRequest(new { status = "invalid_payload" });
    //        }

    //        _logger.LogInformation("Deserialized payload: {Payload}", JsonConvert.SerializeObject(payload));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to deserialize webhook payload.");
    //        return BadRequest(new { status = "invalid_payload_format" });
    //    }

    //    string trxRef = payload.Data.Reference;
    //    string status = payload.Data.Status;
    //    string eventType = payload.Event;  // This is important to identify the event type like "charge.success"

    //    if (string.IsNullOrEmpty(trxRef) || string.IsNullOrEmpty(status))
    //    {
    //        _logger.LogWarning("Transaction reference or status is missing.");
    //        return BadRequest(new { status = "missing_data" });
    //    }

    //    var transaction = await _walletRepository.GetTransactionByTrxRefAsync(trxRef);
    //    if (transaction == null)
    //    {
    //        _logger.LogWarning("Transaction with reference {TrxRef} not found.", trxRef);
    //        return BadRequest(new { status = "transaction_not_found" });
    //    }

    //    // Check if the transaction has been processed already
    //    if (transaction.Status)
    //    {
    //        _logger.LogInformation("Transaction with reference {TrxRef} already processed.", trxRef);
    //        return Ok(new { status = "already_processed" });
    //    }

    //    // Handle charge success or failure
    //    if (eventType == "charge.success" && status == "success")
    //    {
    //        // Assuming the 50 Naira charge needs to be refunded
    //        if (transaction.Amount == 50)
    //        {
    //            // Refund the 50 Naira charge
    //            var refundResult = await _paymentService.RefundTransaction(transaction);
    //            if (refundResult)
    //            {
    //                // Update transaction status after refund
    //                transaction.Status = true;
    //                await _walletRepository.UpdateTransactionAsync(transaction);
    //                _logger.LogInformation("Refund successful for transaction {TrxRef}. Balance updated.");
    //                return Ok(new { status = "success", message = "Refund successful." });
    //            }
    //            else
    //            {
    //                _logger.LogError("Refund failed for transaction {TrxRef}.");
    //                return BadRequest(new { status = "refund_failed", message = "Refund failed." });
    //            }
    //        }
    //    }
    //    else if (eventType == "charge.failed" && status == "failed")
    //    {
    //        _logger.LogWarning("Charge failed for transaction {TrxRef}.");
    //        transaction.Status = false;
    //        await _walletRepository.UpdateTransactionAsync(transaction);
    //        return Ok(new { status = "failure", message = "Charge failed." });
    //    }

    //    return Ok(new { status = "event_not_handled" });
    //}


    //[HttpPost("paystack/webhook")]
    //[AllowAnonymous]
    //public async Task<IActionResult> PaystackWebhook()
    //{
    //    HttpContext.Request.EnableBuffering();
    //    string rawBody;

    //    using (var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
    //    {
    //        rawBody = await reader.ReadToEndAsync();
    //    }

    //    _logger.LogInformation("Webhook received with raw body: {RawBody}", rawBody);
    //    HttpContext.Request.Body.Position = 0;

    //    PaystackWebhookPayload payload;
    //    try
    //    {
    //        payload = JsonConvert.DeserializeObject<PaystackWebhookPayload>(rawBody);
    //        if (payload?.Data == null)
    //        {
    //            _logger.LogError("Invalid webhook payload format.");
    //            return BadRequest(new { status = "invalid_payload" });
    //        }

    //        _logger.LogInformation("Deserialized payload: {Payload}", JsonConvert.SerializeObject(payload));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to deserialize webhook payload.");
    //        return BadRequest(new { status = "invalid_payload_format" });
    //    }

    //    string trxRef = payload.Data.Reference;
    //    string status = payload.Data.Status;

    //    if (string.IsNullOrEmpty(trxRef) || string.IsNullOrEmpty(status))
    //    {
    //        _logger.LogWarning("Transaction reference or status is missing.");
    //        return BadRequest(new { status = "missing_data" });
    //    }

    //    var transaction = await _walletRepository.GetTransactionByTrxRefAsync(trxRef);
    //    if (transaction == null)
    //    {
    //        _logger.LogWarning("Transaction with reference {TrxRef} not found.", trxRef);
    //        return BadRequest(new { status = "transaction_not_found" });
    //    }

    //    if (transaction.Status)
    //    {
    //        _logger.LogInformation("Transaction with reference {TrxRef} already processed.", trxRef);
    //        return Ok(new { status = "already_processed" });
    //    }

    //    if (status == "success")
    //    {
    //        transaction.Status = true;
    //        if (transaction.WalletId != null)
    //        {
    //            var wallet = await _walletRepository.GetWalletByIdAsync(transaction.WalletId);
    //            if (wallet != null)
    //            {
    //                wallet.Balance += transaction.Amount;
    //                await _walletRepository.UpdateWalletAsync(wallet);
    //                _logger.LogInformation("Wallet balance updated for WalletId: {WalletId}.", transaction.WalletId);
    //            }
    //        }

    //        await _walletRepository.UpdateTransactionAsync(transaction);
    //        _logger.LogInformation("Transaction with reference {TrxRef} marked as successful.", trxRef);
    //        return Ok(new { status = "success" });
    //    }

    //    transaction.Status = false;
    //    await _walletRepository.UpdateTransactionAsync(transaction);
    //    _logger.LogInformation("Transaction with reference {TrxRef} marked as failed.", trxRef);
    //    return Ok(new { status = "failure" });
    //}


    //[HttpPost("paystack/save-card")]
    //[Authorize]
    //public async Task<IActionResult> SaveCard([FromBody] StoreCardRequest saveCardRequest)
    //{
    //    // Use the current user's ID
    //    var currentUserId = _currentUserService.GetCurrentUserId();
    //    var currentUser = await _userManager.FindByIdAsync(currentUserId);

    //    if (string.IsNullOrEmpty(currentUser.Email))
    //    {
    //        _logger.LogError("Email is required.");
    //        return BadRequest(new { status = "invalid_data", message = "Email is required." });
    //    }

    //    try
    //    {
    //        // Assuming StoreCardRequest contains authorization code and other details
    //        var cardAuthorizationDetails = new StoreCardRequest
    //        {
    //            AuthorizationCode = saveCardRequest.AuthorizationCode,
    //            Email = currentUser.Email,
    //            CardType = saveCardRequest.CardType,
    //            Last4 = saveCardRequest.Last4,
    //            ExpiryMonth = saveCardRequest.ExpiryMonth,
    //            ExpiryYear = saveCardRequest.ExpiryYear,
    //            Bank = saveCardRequest.Bank,
    //            AccountName = saveCardRequest.AccountName,
    //            Reusable = saveCardRequest.Reusable,
    //            CountryCode = saveCardRequest.CountryCode,
    //            Bin = saveCardRequest.Bin,
    //            Signature = saveCardRequest.Signature,
    //            Channel = saveCardRequest.Channel
    //        };

    //        // Save the card details to the database
    //        await _walletService.StoreCardForRecurringPayment(cardAuthorizationDetails);

    //        _logger.LogInformation("Card saved successfully for user: {UserId}", currentUserId);

    //        // After saving, trigger the Paystack webhook to refund the 50 Naira charge
    //        var transaction = await _walletRepository.GetTransactionByTrxRefAsync(saveCardRequest.TransactionReference);
    //        if (transaction != null && transaction.Amount == 50)
    //        {
    //            await _paymentService.RefundTransaction(transaction);
    //        }

    //        return Ok(new { status = "success", message = "Card saved successfully." });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error saving card details.");
    //        return StatusCode(500, new { status = "error", message = "An error occurred while saving the card details." });
    //    }
    //}



    [HttpGet("paystack/get-stored-card")]
    public async Task<IActionResult> GetCardTokenForRecurringPayment()
    {
        // Use current user service to fetch the current admin username
        var currentUserId = _currentUserService.GetCurrentUserId();
        var currentUser = await _userManager.FindByIdAsync(currentUserId);

        if (string.IsNullOrEmpty(currentUser.Email))
        {
            _logger.LogError("Email is required.");
            return BadRequest(new { status = "invalid_data", message = "Email is required." });
        }

        try
        {
            // Retrieve the stored card authorization details by email
            var storedCard = await _walletRepository.GetCardAuthorizationByEmailAsync(currentUser.Email);
            if (storedCard == null)
            {
                _logger.LogInformation("No card found for email: {Email}", currentUser.Email);
                return NotFound(new { status = "card_not_found", message = "No card found for this email." });
            }

            // Assuming 'AuthorizationCode' or another field is used as a token for recurring payments
            var cardToken = storedCard.AuthorizationCode;

            _logger.LogInformation("Card token retrieved successfully for email: {Email}", currentUser.Email);

            return Ok(new { status = "success", cardToken = cardToken });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving card token.");
            return StatusCode(500, new { status = "error", message = "An error occurred while retrieving the card token." });
        }
    }
    

    //[HttpPost("paystack/webhook")]
    //[AllowAnonymous]
    //public async Task<IActionResult> PaystackWebhook()
    //{
    //    // Enable buffering to allow reading the request body multiple times
    //    HttpContext.Request.EnableBuffering();

    //    // Read raw body from the request
    //    string rawBody;
    //    using (var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
    //    {
    //        rawBody = await reader.ReadToEndAsync();
    //    }

    //    // Log the raw body for debugging purposes
    //    _logger.LogInformation("Raw Webhook Body: {Body}", rawBody);

    //    // Reset the stream position to allow further use of the request body
    //    HttpContext.Request.Body.Position = 0;

    //    // Deserialize the payload
    //    PaystackWebhookPayload payload;
    //    try
    //    {
    //        payload = JsonConvert.DeserializeObject<PaystackWebhookPayload>(rawBody);
    //        if (payload == null || payload.Data == null)
    //        {
    //            _logger.LogError("Webhook payload or Data is null.");
    //            return BadRequest(new ServerResponse<string>
    //            {
    //                IsSuccessful = false,
    //                ResponseCode = "400",
    //                ResponseMessage = "Invalid payload format."
    //            });
    //        }

    //        _logger.LogInformation("Deserialized Paystack webhook payload: {Payload}", JsonConvert.SerializeObject(payload));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to deserialize webhook payload.");
    //        return BadRequest(new ServerResponse<string>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "400",
    //            ResponseMessage = "Invalid payload format."
    //        });
    //    }

    //    // Validate status field
    //    if (string.IsNullOrEmpty(payload.Data.Status))
    //    {
    //        _logger.LogWarning("Status field is null or empty.");
    //        return BadRequest(new ServerResponse<string>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "400",
    //            ResponseMessage = "Invalid payload: Status is required."
    //        });
    //    }

    //    // Store or update transaction
    //    await _paymentService.StoreTransactionAsync(payload);

    //    var trxRef = payload?.Data?.Reference;
    //    var status = payload?.Data?.Status; // status can be 'success' or 'failed'

    //    // Get the transaction from the database
    //    var transaction = await _walletRepository.GetTransactionByTrxRefAsync(trxRef);
    //    if (transaction == null)
    //    {
    //        return BadRequest(new ServerResponse<string>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "404",
    //            ResponseMessage = "Transaction not found."
    //        });
    //    }

    //    // If the status is success, update the transaction and wallet
    //    // If the status is success, update the transaction and make request status
    //    if (status == "success")
    //    {
    //        // Update the transaction status
    //        transaction.Status = true;

    //        // If there is no walletId, it means this transaction is related to a request and not wallet funding
    //        if (transaction.WalletId == null)
    //        {
    //            // Update the associated request (if applicable) to mark it as completed
    //            var request = await _requestRepository.GetRequestByTransactionRefAsync(trxRef);
    //            if (request != null)
    //            {
    //                request.TransactionStatus = true; // Mark the request as completed
    //                await _requestRepository.UpdateRequestAsync(request);
    //            }
    //        }
    //        else
    //        {
    //            // Now update the wallet balance
    //            var wallet = await _walletRepository.GetWalletByIdAsync(transaction.WalletId);
    //            if (wallet != null)
    //            {
    //                wallet.Balance += transaction.Amount;
    //                await _walletRepository.UpdateWalletAsync(wallet);
    //            }
    //        }

    //        // Finally, update the transaction in the database
    //        await _walletRepository.UpdateTransactionAsync(transaction);

    //        return Ok(new { status = "success" });
    //    }

    //    // If the payment failed, you can mark the transaction as failed or handle accordingly
    //    transaction.Status = false;
    //    await _walletRepository.UpdateTransactionAsync(transaction);

    //    return Ok(new { status = "failure" });


    //}


    //[HttpPost("paystack/webhook")]
    //public async Task<IActionResult> PaystackWebhook([FromBody] PaystackWebhookPayload payload, [FromHeader(Name = "x-paystack-signature")] string signature)
    //{
    //    // Log that the webhook endpoint was hit
    //    Console.WriteLine("Paystack webhook triggered.");
    //    _logger.LogInformation("Paystack webhook triggered.");

    //    // Verify Paystack webhook signature
    //    var isValid = _paymentService.VerifyPaystackSignature(payload, signature, _paystackSecretKey);
    //    if (!isValid)
    //    {
    //        Console.WriteLine("Invalid Paystack webhook signature.");
    //        _logger.LogWarning("Invalid Paystack webhook signature.");
    //        return BadRequest(new ServerResponse<string>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "400",
    //            ResponseMessage = "Invalid signature."
    //        });
    //    }

    //    // Log the received payload for debugging purposes
    //    Console.WriteLine("Webhook Payload: " + JsonConvert.SerializeObject(payload));
    //    _logger.LogInformation("Webhook Payload: {Payload}", JsonConvert.SerializeObject(payload));

    //    // Find the transaction in your database
    //    var transaction = await _paymentService.GetTransactionByReferenceAsync(payload.Data.Reference);
    //    if (transaction == null)
    //    {
    //        Console.WriteLine("Transaction not found for reference: " + payload.Data.Reference);
    //        _logger.LogWarning("Transaction not found for reference: {Reference}", payload.Data.Reference);
    //        return NotFound(new ServerResponse<string>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "404",
    //            ResponseMessage = "Transaction not found."
    //        });
    //    }

    //    // Log before updating transaction status
    //    Console.WriteLine($"Updating transaction status for reference: {transaction.TrxRef}");
    //    _logger.LogInformation("Updating transaction status for reference: {Reference}", transaction.TrxRef);

    //    // Update the transaction status
    //    transaction.Status = payload.Data.Status == "success";

    //    // Save the updated transaction
    //    try
    //    {
    //        await _paymentService.UpdateTransactionAsync(transaction);
    //        Console.WriteLine($"Transaction status updated to {transaction.Status} for reference: {transaction.TrxRef}");
    //        _logger.LogInformation("Transaction status updated to {Status} for reference: {Reference}", transaction.Status, transaction.TrxRef);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("Error updating transaction: " + ex.Message);
    //        _logger.LogError(ex, "Error updating transaction for reference: {Reference}", transaction.TrxRef);
    //        return StatusCode(500, new ServerResponse<string>
    //        {
    //            IsSuccessful = false,
    //            ResponseCode = "500",
    //            ResponseMessage = "Internal Server Error. Unable to update transaction status."
    //        });
    //    }

    //    // Return success response to Paystack
    //    Console.WriteLine("Webhook processed successfully.");
    //    _logger.LogInformation("Webhook processed successfully.");
    //    return Ok(new { status = "success" });
    //}

}

