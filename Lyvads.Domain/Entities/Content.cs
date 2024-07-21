
namespace Lyvads.Domain.Entities;

public class Content : Entity
{
    public string Url { get; set; } = default!;
    public bool HasWatermark { get; set; }
    public string UserId { get; set; }
    public Creator Creator { get; set; } = default!;
    public string RequestId { get; set; }
    public Request Request { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
}
