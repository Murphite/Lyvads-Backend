

using Lyvads.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace Lyvads.Application.Dtos.RegularUserDtos;

public class GetRequestDto
{
    public string RequestId { get; set; } = default!;
    public string CreatorFullName { get; set; } = default!;
    public string CreatorProfilePic { get; set; } = default!;
    public string RegularUserFullName { get; set; } = default!;
    public string RegularUserProfilePic { get; set; } = default!;
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetUserRequestDto
{
    public string RequestId { get; set; } = default!;
    public string UserFullName { get; set; } = default!;
    public string UserProfilePic { get; set; } = default!;
    public string CreatorFullName { get; set; } = default!;
    public string CreatorProfilePic { get; set; } = default!;
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}


public class RequestDetailsDto
{
    public string Script { get; set; } = default!;
    public decimal Amount { get; set; }
    public decimal FastTrackFee { get; set; }
    public string CreatorFullName { get; set; } = default!;
    public string CreatorProfilePic { get; set; } = default!; 
    public string RegularUserFullName { get; set; } = default!;
    public string RegularUserProfilePic { get; set; } = default!;
    public string CreatorAppUserName { get; set; } = default!;
    public string RegularUserAppUserName { get; set; } = default!;
    public string RequestId { get; set; } = default!;
    public string? RequestType { get; set; }
    public string? Status { get; set; }
    public string[]? DeclineReason { get; set; }
    public string? DeclineFeedback { get; set; }
    public string? VideoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ChargeTransactionDetailsDto> ChargeTransactions { get; set; } = new();
}


public class ChargeTransactionDetailsDto
{
    public string? ChargeName { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = default!;
}

public class MakeRequestDetailsDto
{
    public string? RequestId { get; set; }
    public string? CreatorId { get; set; }
    public string? CreatorName { get; set; }
    public string? RequestType { get; set; }
    public string? Script { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal Subtotal { get; set; }
    public decimal WithholdingTax { get; set; }
    public decimal WatermarkFee { get; set; }
    public decimal CreatorPostFee { get; set; }
    public decimal FastTrackFee { get; set; }
    public decimal TotalAmount { get; set; }
    //public string? PaymentSummary { get; set; }
    public bool Status { get; set; }
    public string? PaymentReference { get; set; }
    public string? AuthorizationUrl { get; set; }
    public string? CancelUrl { get; set; }

}


public class FeeDetailsDto
{
    public int TotalAmount { get; set; }
    public int WithholdingTax { get; set; }
    public int WatermarkFee { get; set; }
    public int CreatorPostFee { get; set; }
    public int FastTrackFee { get; set; }
}


public class PaymentResponseDto
{
    public string? WalletId { get; set; }
    public decimal? Amount { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public bool Status { get; set; }
    public DateTimeOffset DateCreated { get; set; }
    public string? PaymentReference { get; set; }
    public string? AuthorizationUrl { get; set; }
    public string? CancelUrl { get; set; }
}


public class SubscriptionPaymentResponseDto
{
    public string? SubscriptionId { get; set; }
    public decimal? Amount { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public bool Status { get; set; }
    public DateTimeOffset DateCreated { get; set; }
    public string? PaymentReference { get; set; }
    public string? AuthorizationUrl { get; set; }
    public string? CancelUrl { get; set; }
}



public class StorePaymentCardResponseDto
{
    public string? AuthorizationCode { get; set; }
    public string? Email { get; set; }
    public string? CardType { get; set; }
    public string? Last4 { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
}


public class RefundResponse
{
    public string? Status { get; set; }
    public string? Message { get; set; }
    public RefundData? Data { get; set; }
}

public class RefundData
{
    public string? Status { get; set; }
    public string? Message { get; set; }
    public string? Reference { get; set; }
    public decimal AmountRefunded { get; set; }
}


public class WalletTrasactionResponseDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public int Amount { get; set; }
    public string? TrxRef { get; set; }
    public string? Email { get; set; }
    public bool Status { get; set; }
    public DateTimeOffset DateCreated { get; set; }
    public string? TransactionType { get; set; }

}


 public class PaystackWebhookPayload
    {
        public string Event { get; set; } // "charge.success"
        public PaystackData Data { get; set; }
    }

public class PaystackData
{
    public string? Id { get; set; }
    public string? Domain { get; set; } // "live"
    public string? Status { get; set; } // "success"
    public string? Reference { get; set; }
    public int Amount { get; set; }
    public string? Currency { get; set; } // "NGN"
    public string? Email { get; set; }
    public PaystackCustomer? Customer { get; set; }
    public DateTime TransactionDate { get; set; } // "2024-11-21T14:00:00Z"
    public string? Channel { get; set; } // "card"
    public string? AuthorizationCode { get; set; }
    public string? CardType { get; set; }
    public string? Last4 { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string? Bank { get; set; }
    public string? Bin { get; set; }
    public string? AccountName { get; set; }
    public bool Reusable { get; set; }
    public string? Signature { get; set; }
    public string? CountryCode { get; set; }
}

public class PaystackCustomer
{
    public string? Id { get; set; }
    public string? FirstName { get; set; } // "John"
    public string? LastName { get; set; } // "Doe"
    public string? Email { get; set; }
}

public class DeclineDetailsDto
{
    public string? RequestId { get; set; }
    public string[]? DeclineReason { get; set; }
    public string? Feedback { get; set; }
    public DateTimeOffset DeclinedAt { get; set; }
}


public class CloseRequestDto
{
    public string? RequestId { get; set; }
}


public class ResendRequestDto
{
    public string? RequestId { get; set; }
    public string? Script { get; set; }
    public string? RequestType { get; set; }
}

public class ResendResponseDto
{
    public string? RequestId { get; set; }
    public string? Script { get; set; }
    public string? RequestType { get; set; }
    public string? UpdatedStatus { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}


public class CreatorCollaborationDto
{
    public string? CreatorId { get; set; }
    public string? CreatorName { get; set; }
    public int CompletedJobsCount { get; set; }
}



public class CloseRequestResultDto
{
    public string? RequestId { get; set; }
    public string? RequestStatus { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public decimal UserWalletBalance { get; set; }
    public decimal CreatorWalletBalance { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal ReversedAmount { get; set; }
}

public class StoreCardRequest
{
    public string? AuthorizationCode { get; set; }
    public string? Email { get; set; }
    public string? CardType { get; set; }
    public string? Last4 { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string? Bank { get; set; }
    public string? Bin { get; set; }
    public string? Signature { get; set; }
    public string? AccountName { get; set; }
    public string? Channel { get; set; }
    public bool Reusable { get; set; }
    public string? CountryCode { get; set; }
    public string? TransactionReference { get; set; }
}

public class RefundRequest
{
    public string? TransactionReference { get; set; }
}
