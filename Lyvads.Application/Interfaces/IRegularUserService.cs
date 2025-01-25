
using Lyvads.Shared.DTOs;
using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.CreatorDtos;
using Lyvads.Domain.Responses;

namespace Lyvads.Application.Interfaces;

public interface IRegularUserService
{
    Task<ServerResponse<PaginatorDto<IEnumerable<RegularUserDto>>>> GetRegularUsers(PaginationFilter paginationFilter);
    Task<Result<RegularUserProfileResponseDto>> UpdateUserProfileAsync(UpdateRegularUserProfileDto dto, string userId);
}
