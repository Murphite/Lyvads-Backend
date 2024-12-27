

using Lyvads.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Dispute : Entity, IAuditable
{
    public string? RequestId{ get; set; }
    [ForeignKey("RegularUser")]
    public string? RegularUserId { get; set; }
    [ForeignKey("Creator")]
    public string? CreatorId { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }
    public string DisputeMessage { get; set; } = default!;
    public DisputeReasons Reason { get; set; }
    public DisputeStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string ApplicationUserId { get; set; } = default!;
    public ApplicationUser ApplicationUser { get; set; } = default!;
    public virtual RegularUser RegularUser { get; set; } = default!;
    public virtual Creator Creator { get; set; } = default!;
    public Request? Request { get; set; }

}
