

namespace Lyvads.Application.Dtos.AuthDtos;

public class ForgotPasswordRequestDto
{
    public string? Email { get; set; }
}

public class ResetPasswordWithCodeDto
{
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}

public class AdminResetPasswordWithCodeDto
{
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}

public class PasswordResetResponseDto
{
    public string? Email { get; set; }
    public bool IsPasswordReset { get; set; } = true;
    public string? Message { get; set; }
    public string? NewPassword { get; set; }

}