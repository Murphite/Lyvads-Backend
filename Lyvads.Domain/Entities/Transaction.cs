
using Lyvads.Domain.Enums;

namespace Lyvads.Domain.Entities;

public class Transaction : Entity
{
    public string? WalletId { get; set; }
    public Wallet Wallet { get; set; } = default!;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public string? Description { get; set; }

    // Optional: reference to the related deal or request, if applicable
    public string? DealId { get; set; }
    public Deal? Deal { get; set; }

    public string? RequestId { get; set; }
    public Request? Request { get; set; }

    // Optional: references for transfer transactions
    public string? FromWalletId { get; set; }
    public Wallet? FromWallet { get; set; }

    public string? ToWalletId { get; set; }
    public Wallet? ToWallet { get; set; }
}
