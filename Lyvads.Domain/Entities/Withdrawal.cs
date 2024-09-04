

namespace Lyvads.Domain.Entities;

public class Withdrawal : Entity, IAuditable
{
    public string? UserId { get; set; } 
    public decimal Amount { get; set; } 
    public string? TransferReference { get; set; } 
    public TransferStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ApplicationUser? User { get; set; }
}


public enum TransferStatus
{
    Pending, 
    Completed,
    Failed
}