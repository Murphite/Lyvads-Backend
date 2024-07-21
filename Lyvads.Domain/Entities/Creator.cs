
namespace Lyvads.Domain.Entities;

public class Creator : ApplicationUser
{
    public ICollection<Content> Contents { get; set; } = new List<Content>();
    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();

    // Social Media Handles
    public string? FacebookHandle { get; set; }
    public string? InstagramHandle { get; set; }
    public string? TwitterHandle { get; set; }
    public string? TikTokHandle { get; set; }

    // Exclusive Deal Properties
    public bool HasExclusiveDeal { get; set; }
    public ICollection<ExclusiveDeal> ExclusiveDeals { get; set; } = new List<ExclusiveDeal>();
}

public class ExclusiveDeal : Entity
{
    public string Industry { get; set; } = default!;
    public string BrandName { get; set; } = default!;
    public string CreatorId { get; set; } = default!;
    public Creator Creator { get; set; } = default!;
}
