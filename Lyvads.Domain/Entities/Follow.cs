

using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Follow : Entity, IAuditable
{
    public string? ApplicationUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } 
    public DateTimeOffset UpdatedAt { get; set; }
    public string? CreatorId { get; set; } 
    
    
    // Navigation properties 
    public virtual ApplicationUser? ApplicationUser { get; set; } 
    public virtual Creator? Creator { get; set; } 
}