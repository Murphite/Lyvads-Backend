

using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Interfaces;

public interface IAdminPermissionsService
{
    Task<ServerResponse<List<AdminUserDto>>> GetAllAdminUsersAsync();
    Task<ServerResponse<AdminPermissionsDto>> GrantPermissionsToAdminAsync(string adminUserId,
        AdminPermissionsDto permissionsDto, string requestingAdminId);
    Task<ServerResponse<string>> CreateCustomRoleAsync(string roleName, AdminPermissionsDto permissionsDto);
    Task<ServerResponse<EditResponseAdminUserDto>> EditAdminUserAsync(string adminUserId, EditAdminUserDto editAdminUserDto);
    Task<ServerResponse<AddAdminUserResponseDto>> AddAdminUserAsync(AddAdminUserDto addAdminUserDto);
    Task<ServerResponse<string>> DeleteAdminUserAsync(string userId);
}
