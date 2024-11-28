

using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Interfaces;

public interface IUserAdService
{
    Task<ServerResponse<List<UserAdDto>>> GetAllUserAdsAsync();
    Task<ServerResponse<string>> ApproveAdAsync(string adId);
    Task<ServerResponse<string>> DeclineAdAsync(string adId);
    Task<ServerResponse<string>> ToggleAdStatusAsync(string adId);
    Task<ServerResponse<AddUserAdResponseDto>> AddUserAdAsync(AddUserAdDto userAdDto);
    Task<ServerResponse<EditUserAdResponseDto>> EditUserAdAsync(string adId, EditUserAdDto editUserAdDto);
}
