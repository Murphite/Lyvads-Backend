

namespace Lyvads.Domain.Entities;

public class WaitlistEntry : Entity
{
    public string? Email { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
