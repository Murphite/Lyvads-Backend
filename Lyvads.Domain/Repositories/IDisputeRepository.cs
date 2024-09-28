
using Lyvads.Domain.Entities;
using Lyvads.Domain.Responses;

namespace Lyvads.Domain.Repositories;

public interface IDisputeRepository
{
    Task<List<Dispute>> GetAllAsync();
    Task<Dispute> GetByIdAsync(string id);
    Task<ServerResponse<bool>> CreateDispute(Dispute dispute);
    IQueryable<Dispute> GetDisputesByCreator(string creatorId);
    Task<Dispute> GetDisputeById(string disputeId);
}
