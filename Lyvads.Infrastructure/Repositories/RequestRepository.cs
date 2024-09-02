using Lyvads.Infrastructure.Persistence;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;

namespace Lyvads.Infrastructure.Repositories;

public class RequestRepository : IRequestRepository
{
    private readonly AppDbContext _context;

    public RequestRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<(bool IsSuccess, string ErrorMessage)> CreateRequestAsync(Request request)
    {
        try
        {
            // Add the request to the database context
            await _context.Requests.AddAsync(request);

            // Save changes asynchronously
            await _context.SaveChangesAsync();

            // Return a success result
            return (true, null);
        }
        catch (Exception ex)
        {
            // Log the exception if needed
            // _logger.LogError(ex, "Error creating request");

            // Return a failure result with the exception message
            return (false, ex.Message);
        }
    }

}
