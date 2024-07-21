using Lyvads.Domain.Entities;

namespace Lyvads.Domain.Interfaces;

public interface IJwtService
{
    public string GenerateToken(ApplicationUser user, IList<string> roles);
}
