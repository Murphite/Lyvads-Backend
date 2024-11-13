

using Lyvads.Application.Dtos.AuthDtos;
using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Interfaces;

public interface ISuperAdminService
{
    Task<ServerResponse<List<UserDto>>> GetUsers(string role = null!, bool sortByDate = true);
    Task<ServerResponse<AddUserResponseDto>> AddUser(AdminRegisterUserDto registerUserDto);  
    Task<ServerResponse<string>> UpdateUser(UpdateUserDto updateUserDto, string userId);
    Task<ServerResponse<string>> DeleteUser(string userId);
    Task<ServerResponse<string>> DisableUser(string userId);
    Task<ServerResponse<string>> ActivateUserAsync(string userId);
    Task<ServerResponse<string>> ToggleUserStatusAsync(string userId);

}
