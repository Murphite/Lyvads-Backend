

namespace Lyvads.Domain.Entities;

public class Transfer : Entity, IAuditable
{
    public required string UserId { get; set; } 
    public required decimal Amount { get; set; } 
    public required string TransferReference { get; set; } 
    public required string Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public required ApplicationUser User { get; set; }
}
