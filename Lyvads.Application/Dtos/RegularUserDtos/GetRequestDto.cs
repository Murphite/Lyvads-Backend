

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
    public string CreatorAppUserName { get; set; } = default!;
    public string RequestId { get; set; } = default!;
    public string? RequestType { get; set; }
    public string? Status { get; set; }
    public decimal FastTractFee { get; set; }
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
    public string? PaymentSummary { get; set; }
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
    public int? Amount { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public bool Status { get; set; }
    public DateTimeOffset DateCreated { get; set; }
    public string? PaymentReference { get; set; }
    public string? AuthorizationUrl { get; set; }
    public string? CancelUrl { get; set; }
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
        public string Id { get; set; }
        public string Domain { get; set; } // "live"
        public string Status { get; set; } // "success"
        public string Reference { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; } // "NGN"
        public string Email { get; set; }
        public PaystackCustomer Customer { get; set; }
        public DateTime TransactionDate { get; set; } // "2024-11-21T14:00:00Z"
        public string Channel { get; set; } // "card"
    }

    public class PaystackCustomer
    {
        public long Id { get; set; }
        public string FirstName { get; set; } // "John"
        public string LastName { get; set; } // "Doe"
        public string Email { get; set; }
    }