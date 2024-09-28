

using Lyvads.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Lyvads.Application.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUserId()
    {
        // Retrieve the user ID from the HttpContext
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId ?? string.Empty; // Return an empty string if userId is null
    }

}
