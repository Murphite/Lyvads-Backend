

using Lyvads.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Transaction : Entity, IAuditable
{
    public string? Name { get; set; }
    public int Amount { get; set; }
    public string? TrxRef { get; set; }
    public string? Email { get; set; }
    public bool Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string? RequestId { get; set; }
    public Request? Request { get; set; }

    public string? WalletId { get; set; }
    public Wallet Wallet { get; set; } = default!;

    public string? ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = default!;

    public ICollection<ChargeTransaction> ChargeTransactions { get; set; } = new List<ChargeTransaction>();

    public TransactionType Type { get; set; }
}
