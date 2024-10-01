using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyvads.Application.Dtos.AuthDtos;

public class ForgotPasswordRequestDto
{
    public string Email { get; set; }
}

public class ResetPasswordWithCodeDto
{
    public string? Email { get; set; }
    public string? VerificationCode { get; set; }
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