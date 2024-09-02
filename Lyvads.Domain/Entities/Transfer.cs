

namespace Lyvads.Domain.Entities;

public class Transfer : Entity, IAuditable
{
    public string UserId { get; set; } 
    public decimal Amount { get; set; } 
    public string TransferReference { get; set; } 
    public string Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ApplicationUser User { get; set; }
}
