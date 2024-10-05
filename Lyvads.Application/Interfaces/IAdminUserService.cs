

using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Interfaces;

public interface IAdminUserService
{
    Task<ServerResponse<AddUserResponseDto>> RegisterAdmin(RegisterAdminDto registerAdminDto);
    Task<ServerResponse<DashboardSummaryDto>> GetDashboardSummary();
    Task<ServerResponse<RevenueReportDto>> GetRevenueReport();
    Task<ServerResponse<List<TopRequestDto>>> GetTopRequests();
    Task<ServerResponse<List<TopCreatorDto>>> GetTopCreators();
    Task<ServerResponse<CollaborationStatusReportDto>> GetCollaborationStatusesReport();
}
