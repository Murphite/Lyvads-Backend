

namespace Lyvads.Domain.Entities;

public class Entity
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
}
