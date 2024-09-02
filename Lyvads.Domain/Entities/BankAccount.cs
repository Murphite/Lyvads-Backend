
namespace Lyvads.Domain.Entities;

public class BankAccount : Entity, IAuditable
{
    public string UserId { get; set; } 
    public string BankName { get; set; } 
    public string AccountNumber { get; set; } 
    public string AccountHolderName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation property for the User entity
    public virtual ApplicationUser User { get; set; }
}
