

using Lyvads.Domain.Enums;

namespace Lyvads.Domain.Entities;

public class Creator : Entity, IAuditable
{
    public string? UserId { get; set; }
    public ICollection<Content> Contents { get; set; } = new List<Content>();
    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();

    // Social Media Handles
    public string? SimpleAdvert { get; set; }
    public string? WearBrand { get; set; }
    public string? SongAdvert { get; set; }
    public RequestType? Request { get; set; }

    // Exclusive Deal Properties
    public bool HasExclusiveDeal { get; set; }
    public ICollection<ExclusiveDeal> ExclusiveDeals { get; set; } = new List<ExclusiveDeal>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    //navigation property   
    public ApplicationUser? ApplicationUser { get; set; }
}

public class ExclusiveDeal : Entity
{
    public string Industry { get; set; } = default!;
    public string BrandName { get; set; } = default!;
    public string CreatorId { get; set; } = default!;
    public Creator Creator { get; set; } = default!;
}
