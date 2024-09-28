using Lyvads.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Collaboration : Entity, IAuditable
{
    public string? RequestId { get; set; }
    public string? RegularUserId { get; set; }
    public string? CreatorId { get; set; }
    public string VideoUrl { get; set; } = default!;
    public string UserResponse { get; set; } = default!;
    public string DisputeReason { get; set; } = default!;
    public string? ReceiptUrl { get; set; }
    public string? Details { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public required decimal Amount { get; set; }
    public CollaborationStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public RegularUser? RegularUser { get; set; } // Corrected property name
    public Creator? Creator { get; set; }
}
