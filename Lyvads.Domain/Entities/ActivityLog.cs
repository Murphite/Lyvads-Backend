

using Lyvads.Domain.Constants;

namespace Lyvads.Domain.Entities;

public class ActivityLog : Entity
{
    public string? UserName { get; set; } 
    public string? Role { get; set; }
    public string? Description { get; set; } 
    public string? Category { get; set; }
    public string ApplicationUserId { get; set; } = default!;
    public ApplicationUser ApplicationUser { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

