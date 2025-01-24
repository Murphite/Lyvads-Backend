

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
    public decimal Amount { get; set; } = default!;
    public string? paymentMethodId { get; set; } = default!;
    public string? currency { get; set; } = default!;
}


public class CreatorProfileDto
{
    public string Id { get; set; } = default!;
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public string? AppUserName { get; set; }
    public int FollowersCount { get; set; }
    public int EngagementCount { get; set; }
    public List<ViewPostDto>? Posts { get; set; }
    public string? Bio { get; set; }
    public string? Occupation { get; set; }
    public string? Location { get; set; }
    public List<CollabRateDto>? CollabRates { get; set; }
}


public class CollabRateDto
{
    public string RateId { get; set; } = default!;
    public string? RequestType { get; set; }
    public decimal TotalAmount { get; set; }
}

public class GetPostDto
{
    public string? PostId { get; set; }    
    public string? CreatorName { get; set; }
    public string? Caption { get; set; }
    public string? Location { get; set; }
    public string? Visibility { get; set; }   
    public DateTimeOffset CreatedAt { get; set; }
    public List<string>? MediaUrls { get; set; }
}

public class UserWithFollowersDto
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public List<FollowerDto>? Followers { get; set; }
    public List<FollowerDto>? Following { get; set; }
}


public class FollowerDto
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? CreatorId { get; set; }
    public string? CreatorName { get; set; }
}



public class UserFollowerDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
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


