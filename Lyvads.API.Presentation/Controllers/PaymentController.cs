using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.RegularUserDtos;
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
    private readonly IRequestRepository _requestRepository;

    public PaymentController(
        IConfiguration configuration,
        IWalletService walletService,
        IWalletRepository walletRepository,
        IRequestRepository requestRepository,
        UserManager<ApplicationUser> userManager,
        IPaymentGatewayService paymentService,
        ILogger<PaymentController> logger)
    {
        _configuration = configuration;
        _walletService = walletService;
        _userManager = userManager;
        _paymentService = paymentService;
        _logger = logger;
        _paystackSecretKey = _configuration["Paystack:PaystackSK"];
        _walletRepository = walletRepository;
        _requestRepository = requestRepository;
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


    [HttpPost("paystack/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PaystackWebhook()
    {
        // Enable buffering to allow reading the request body multiple times
        HttpContext.Request.EnableBuffering();

        // Read raw body from the request
        string rawBody;
        using (var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            rawBody = await reader.ReadToEndAsync();
        }

        // Log the raw body for debugging purposes
        _logger.LogInformation("Raw Webhook Body: {Body}", rawBody);

        // Reset the stream position to allow further use of the request body
        HttpContext.Request.Body.Position = 0;

        // Deserialize the payload
        PaystackWebhookPayload payload;
        try
        {
            payload = JsonConvert.DeserializeObject<PaystackWebhookPayload>(rawBody);
            if (payload == null || payload.Data == null)
            {
                _logger.LogError("Webhook payload or Data is null.");
                return BadRequest(new ServerResponse<string>
                {
                    IsSuccessful = false,
                    ResponseCode = "400",
                    ResponseMessage = "Invalid payload format."
                });
            }

            _logger.LogInformation("Deserialized Paystack webhook payload: {Payload}", JsonConvert.SerializeObject(payload));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize webhook payload.");
            return BadRequest(new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Invalid payload format."
            });
        }

        // Validate status field
        if (string.IsNullOrEmpty(payload.Data.Status))
        {
            _logger.LogWarning("Status field is null or empty.");
            return BadRequest(new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "400",
                ResponseMessage = "Invalid payload: Status is required."
            });
        }

        // Store or update transaction
        await _paymentService.StoreTransactionAsync(payload);

        var trxRef = payload?.Data?.Reference;
        var status = payload?.Data?.Status; // status can be 'success' or 'failed'

        // Get the transaction from the database
        var transaction = await _walletRepository.GetTransactionByTrxRefAsync(trxRef);
        if (transaction == null)
        {
            return BadRequest(new ServerResponse<string>
            {
                IsSuccessful = false,
                ResponseCode = "404",
                ResponseMessage = "Transaction not found."
            });
        }

        // If the status is success, update the transaction and wallet
        // If the status is success, update the transaction and make request status
        if (status == "success")
        {
            // Update the transaction status
            transaction.Status = true;

            // If there is no walletId, it means this transaction is related to a request and not wallet funding
            if (transaction.WalletId == null)
            {
                // Update the associated request (if applicable) to mark it as completed
                var request = await _requestRepository.GetRequestByTransactionRefAsync(trxRef);
                if (request != null)
                {
                    request.TransactionStatus = true; // Mark the request as completed
                    await _requestRepository.UpdateRequestAsync(request);
                }
            }
            else
            {
                // Now update the wallet balance
                var wallet = await _walletRepository.GetWalletByIdAsync(transaction.WalletId);
                if (wallet != null)
                {
                    wallet.Balance += transaction.Amount;
                    await _walletRepository.UpdateWalletAsync(wallet);
                }
            }

            // Finally, update the transaction in the database
            await _walletRepository.UpdateTransactionAsync(transaction);

            return Ok(new { status = "success" });
        }

        // If the payment failed, you can mark the transaction as failed or handle accordingly
        transaction.Status = false;
        await _walletRepository.UpdateTransactionAsync(transaction);

        return Ok(new { status = "failure" });


    }

}

