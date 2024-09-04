

using Lyvads.Domain.Enums;

namespace Lyvads.Domain.Entities;

public class CollaborationRequest : Entity, IAuditable
{
    public string? RequestId { get; set; }
    public string? UserId { get; set; }
    public string? CreatorId { get; set; }
    public string VideoUrl { get; set; } = default!;
    public string UserResponse { get; set; } = default!;
    public string DisputeReason { get; set; } = default!;
    public RequestStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; } 
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public ApplicationUser? User { get; set; }
    public Creator? Creator { get; set; } 
}

//public enum RequestStatus
//{
//    Pending,    // The request is made and awaiting action
//    Completed,  // The request has been fulfilled
//    Disputed    // The request is disputed by the user
//}
