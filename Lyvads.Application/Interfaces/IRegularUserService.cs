

using Lyvads.Application.Dtos.RegularUserDtos;
using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.CreatorDtos;

namespace Lyvads.Application.Interfaces;

public interface IRegularUserService
{
    Task<Result<UpdateProfilePicResponseDto>> UpdateProfilePictureAsync(string userId, string newProfilePictureUrl);
    Task<Result<RegularUserProfileResponseDto>> UpdateUserProfileAsync(UpdateRegularUserProfileDto dto, string userId);
    Task<Result<EditProfileResponseDto>> EditProfileAsync(EditProfileDto dto, string userId);
}
