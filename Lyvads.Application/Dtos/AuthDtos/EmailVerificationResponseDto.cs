
namespace Lyvads.Application.Dtos.AuthDtos;

public class EmailVerificationResponseDto
{
    public string? Email { get; set; }
    public bool IsVerified { get; set; }
    public string? Message { get; set; }
}
