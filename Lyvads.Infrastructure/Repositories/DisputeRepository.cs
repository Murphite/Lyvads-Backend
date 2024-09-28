using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace Lyvads.Infrastructure.Repositories;

public class DisputeRepository : IDisputeRepository
{
    private readonly AppDbContext _context;

    public DisputeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Dispute>> GetAllAsync()
    {
        return await _context.Disputes
            .Include(d => d.RegularUser)
            .Include(d => d.Creator)
            .ToListAsync();
    }

    public async Task<Dispute> GetByIdAsync(string id)
    {
        var disputeID = await _context.Disputes
            .Include(d => d.RegularUser)
            .Include(d => d.Creator)
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
                       .Include(d => d.RequestId) 
                       .Where(d => d.CreatorId == creatorId); 
    }

    public async Task<Dispute> GetDisputeById(string disputeId)
    {
        var getDisputeById =  await _context.Disputes.FindAsync(disputeId);

        if (getDisputeById == null)
            throw new Exception("Dispute Id Not Found");

        return getDisputeById;
    }
}