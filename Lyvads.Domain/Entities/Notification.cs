

namespace Lyvads.Domain.Entities;

public class Notification : Entity
{
    public string? Message { get; set; } = default!;
    public string? Content { get; set; }
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public bool Seen { get; set; } = false; // Optional: Track if notification has been read
}