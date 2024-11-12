using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Lyvads.Infrastructure.Repositories;

public class DisputeRepository : IDisputeRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<DisputeRepository> _logger;    

    public DisputeRepository(AppDbContext context,
        ILogger<DisputeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Dispute>> GetAllAsync()
    {
        return await _context.Disputes
            .Include(d => d.RegularUser)
                .ThenInclude(ru => ru.ApplicationUser) 
            .Include(d => d.Creator)
                .ThenInclude(c => c.ApplicationUser) 
            .ToListAsync();
    }


    public async Task<Dispute> GetByIdAsync(string id)
    {
        var disputeID = await _context.Disputes
            .Include(d => d.RegularUser)
                .ThenInclude(ru => ru.ApplicationUser)
            .Include(d => d.Creator)
                .ThenInclude(c => c.ApplicationUser)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (disputeID == null)
            throw new Exception("Dispute Id Not Found");

        return disputeID;
    }

    public async Task<ServerResponse<bool>> CreateDispute(Dispute dispute)
    {
        try
        {
            await _context.Disputes.AddAsync(dispute);
            await _context.SaveChangesAsync();

            return new ServerResponse<bool>
            {
                IsSuccessful = true,
                Data = true,
                ResponseCode = "00",
                ResponseMessage = "Dispute created successfully."
            };
        }
        catch (Exception ex)
        {
            return new ServerResponse<bool>
            {
                IsSuccessful = false,
                ResponseCode = "500",
                ResponseMessage = $"Error creating dispute: {ex.Message}"
            };
        }
    }


    public IQueryable<Dispute> GetDisputesByCreator(string creatorId)
    {
        return _context.Disputes
                       //.Include(d => d.ApplicationUser) 
                       .Where(d => d.CreatorId == creatorId); 
    }

    public async Task<Dispute> GetDisputeById(string disputeId)
    {
        var dispute = await _context.Disputes.Include(d => d.ApplicationUser).FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            // Log or throw a more informative error
            _logger.LogWarning("Dispute with ID {DisputeId} not found.", disputeId);
            throw new Exception("Dispute ID not found");
        }

        return dispute;
    }
}