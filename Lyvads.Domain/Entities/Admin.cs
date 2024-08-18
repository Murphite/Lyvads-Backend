

namespace Lyvads.Domain.Entities;

public class Admin : Entity, IAuditable
{
    public string UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ApplicationUser ApplicationUser { get; set; }
}
