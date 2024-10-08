

using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Follow : Entity, IAuditable
{
    public string? UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } // Date and time when the follow action occurred
    public DateTimeOffset UpdatedAt { get; set; }
    public string? FollowerId { get; set; } // Identifier for the user who is following
    public string? CreatorId { get; set; } // Identifier for the creator being followed
    
    
    // Navigation properties 
    public virtual ApplicationUser? Follower { get; set; } // Reference to the user entity who is following
    public virtual Creator? FollowedCreator { get; set; } // Reference to the creator entity being followed
}