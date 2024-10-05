
using Lyvads.Domain.Enums;

namespace Lyvads.Domain.Entities;

public class Impression : Entity, IAuditable
{
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public string CreatorId { get; set; } = default!;
    public Creator Creator { get; set; } = default!; 

    public string? ContentId { get; set; }
    public ContentType ContentType { get; set; } 

    public DateTimeOffset ViewedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
