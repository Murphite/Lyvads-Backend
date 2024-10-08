

using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Shared.DTOs;

public class CommentDto
{
    public string? UserId { get; set; } = default!;
    public string? Content { get; set; } = default!;
}

public class LikeDto
{
    public string? UserId { get; set; } = default!;
    public string? ContentId { get; set; } = default!;
}

public class FundWalletDto
{
    public string? UserId { get; set; } = default!;
    public decimal Amount { get; set; } = default!;
}


public class CreatorProfileDto
{
    public string Id { get; set; } = default!;
    public string? Name { get; set; }
    public string? AppUserName { get; set; }
    public int FollowersCount { get; set; }
    public int EngagementCount { get; set; }
    public List<ViewPostDto>? Posts { get; set; }
    public string? Bio { get; set; }
    public string? Occupation { get; set; }
    public List<CollabRateDto>? CollabRates { get; set; }
}


public class CollabRateDto
{
    public string? RequestType { get; set; }
    public decimal TotalAmount { get; set; }
}

public class ViewPostDto
{
    public string? Id { get; set; }
    public string? Caption { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ViewPostResponseDto
{
    public string? PostId { get; set; }
    public string? Caption { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}



public class CreatorResponseDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? ProfilePicture { get; set; }
    public string? Industry { get; set; } // Optional, hence nullable
    public string? AppUserName { get; set; } // Optional, hence nullable
    public int FollowersCount { get; set; } // Assuming this exists in your previous method
    public int EngagementCount { get; set; } // Assuming this exists in your previous method
}


public class FeaturedCreatorDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public int EngagementCount { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Industry { get; set; } // Optional, hence nullable
    public string? AppUserName { get; set; } // Optional, hence nullable
    public int FollowersCount { get; set; } // Assuming this exists in your previous method
}


