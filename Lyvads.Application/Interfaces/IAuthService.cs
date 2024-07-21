using Lyvads.Application.Dtos;
using Lyvads.Application.Dtos.AuthDtos;

namespace Lyvads.Application.Interfaces;

public interface IAuthService
{
    public Task<Result> RegisterUser(RegisterUserDto registerUserDto);
    public Task<Result> RegisterCreator(RegisterCreatorDto registerCreatorDto);
    public Task<Result> RegisterAdmin(RegisterAdminDto registerAdminDto);
    public Task<Result<LoginResponseDto>> Login(LoginUserDto loginUserDto);
    public Task<Result> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    public Task<Result> ForgotPassword(ResetPasswordDto resetPasswordDto);
    public Task<Result> ConfirmEmail(string email, string token);
    public Task<Result> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
}