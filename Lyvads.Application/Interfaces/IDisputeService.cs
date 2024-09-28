

using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Interfaces;

public interface IDisputeService
{
    Task<ServerResponse<List<DisputeDto>>> GetAllDisputesAsync();
    Task<ServerResponse<DisputeDto>> GetDisputeDetailsAsync(string disputeId);
}
