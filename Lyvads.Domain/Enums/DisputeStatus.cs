
using System.ComponentModel.DataAnnotations;

namespace Lyvads.Domain.Enums;

public enum DisputeStatus
{
    Pending,
    Resolved,
    InReview
}


public enum DisputeReasons
{
    VideoIsNotAccepted,
    NoisyBackground,
    PoorVideoQuality,
    OneLastOne,
    OneMore
}


public enum DisputeType
{
    DisputedVideo,
    DeclinedRequest
}

public class DeclineRequestDto
{
    public string? RequestId { get; set; }

    [Required]
    public string[] DeclineReasons { get; set; } = Array.Empty<string>(); 

    public string? Feedback { get; set; }
}


public class DeclineResponseDto
{
    public string? UserId { get; set; }
    public string? RequestId { get; set; }
    public string? Status { get; set; }
    public string? DeclineReason { get; set; }
    public string? Feedback { get; set; }
}