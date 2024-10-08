
namespace Lyvads.Domain.Entities;

public class Favorite : Entity, IAuditable
{
    public string? UserId { get; set; } 
    public string? CreatorId { get; set; } 
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public virtual ApplicationUser? User { get; set; } 
    public virtual Creator? Creator { get; set; }
}