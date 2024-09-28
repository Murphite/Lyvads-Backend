
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