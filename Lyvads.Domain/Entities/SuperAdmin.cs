

namespace Lyvads.Domain.Entities;

public class SuperAdmin : Entity, IAuditable
{
    public string? UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
}
