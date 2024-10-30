

using Lyvads.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyvads.Domain.Entities;

public class Creator : Entity, IAuditable
{
    public ICollection<Content> Contents { get; set; } = new List<Content>();
    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Collaboration> Collaborations { get; set; } = new List<Collaboration>();
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    // Social Media Handles
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }
    public string? XTwitter { get; set; }
    public string? Tiktok { get; set; }


    // Creator SetUp Rates
    public string? SimpleAdvert { get; set; }
    public string? WearBrand { get; set; }
    public string? SongAdvert { get; set; }
    public string? Request { get; set; }
    

    // Exclusive Deal Properties
    public bool HasExclusiveDeal { get; set; }
    public ICollection<ExclusiveDeal> ExclusiveDeals { get; set; } = new List<ExclusiveDeal>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal AdvertAmount { get; set; }


    // Navigation Property   
    public string ApplicationUserId { get; set; } = default!;
    public ApplicationUser? ApplicationUser { get; set; }

    [NotMapped]
    public int EngagementCount => Posts.Sum(p => p.Likes.Count + p.Comments.Count);
    public ICollection<CollaborationRate>? CollabRates { get; set; }

}

public class ExclusiveDeal : Entity
{
    public string Industry { get; set; } = default!;
    public string BrandName { get; set; } = default!;
    public string CreatorId { get; set; } = default!;
    public Creator Creator { get; set; } = default!;
}


public class CollaborationRate : Entity
{
    public RequestType RequestType { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Rate { get; set; } // Rate for the request

    public string? CreatorId { get; set; }
    public Creator? Creator { get; set; }
}
