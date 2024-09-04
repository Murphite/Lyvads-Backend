using Lyvads.Infrastructure.Persistence;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Lyvads.Infrastructure.Repositories;

public class RequestRepository : IRequestRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<RequestRepository> _logger;

    public RequestRepository(AppDbContext context, ILogger<RequestRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<(bool IsSuccess, string ErrorMessage)> CreateRequestAsync(Request request)
    {
        try
        {
            await _context.Requests.AddAsync(request);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating request");

            return (false, ex.Message ?? string.Empty);
        }
    }

}
