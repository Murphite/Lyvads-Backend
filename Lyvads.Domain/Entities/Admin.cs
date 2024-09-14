

namespace Lyvads.Domain.Entities;

public class Admin : Entity, IAuditable
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string ApplicationUserId { get; set; } = default!;
    public ApplicationUser? ApplicationUser { get; set; }
}
