using Lyvads.Infrastructure.Persistence;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

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

    public IQueryable<Request> GetRequests()
    {
        return _context.Requests; 
    }

    public async Task<Request> GetRequestById(string requestId)
    {
        return await _context.Requests.FirstOrDefaultAsync(r => r.Id == requestId);
    }

    public async Task<Request?> GetRequestByIdAsync(string requestId)
    {
        return await _context.Requests
            .Include(r => r.Creator)
                .ThenInclude(c => c.ApplicationUser)
            .Include(r => r.RegularUser)
                .ThenInclude(c => c.ApplicationUser)
            .Include(r => r.Transactions)
                .ThenInclude(t => t.ChargeTransactions) 
            .FirstOrDefaultAsync(r => r.Id == requestId);
    }

    public IQueryable<Request> GetRequestsForCreator(string creatorId)
    {
        return _context.Requests
                       .Include(r => r.Creator)
                       .Include(r => r.RegularUser)
                            .ThenInclude(c => c.ApplicationUser)                       
                       .Where(r => r.CreatorId == creatorId);
    }

    // Method to get requests made by a specific user
    public IQueryable<Request> GetRequestsByUser(string userId)
    {
        return _context.Requests
                       .Include(r => r.RegularUser) // Include RegularUser for navigation
                       .Include(r => r.Creator)
                            .ThenInclude(c => c.ApplicationUser)// Include Creator if needed
                       .Where(r => r.RegularUserId == userId); // Filter by RegularUserId
    }

    public async Task<Request> GetRequestByTransactionRefAsync(string trxRef)
    {
        var request = await _context.Requests
            .FirstOrDefaultAsync(r => r.Transactions.Any(t => t.TrxRef == trxRef));

        return request;
    }



    public async Task UpdateRequestAsync(Request request)
    {
        if (request != null)
        {
            _context.Requests.Update(request);

            await _context.SaveChangesAsync();
        }
    }

}
