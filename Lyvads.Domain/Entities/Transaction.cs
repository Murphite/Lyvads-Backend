

using Lyvads.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Transaction : Entity , IAuditable
{
    public string? WalletId { get; set; }
    public Wallet Wallet { get; set; } = default!;
    public TransactionType Type { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public string? Description { get; set; }

    // Optional: reference to the related deal or request, if applicable
    public string? DealId { get; set; }
    public Deal? Deal { get; set; }

    public string? RequestId { get; set; }
    public Request? Request { get; set; }

    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;

    public ApplicationUser? Sender { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
