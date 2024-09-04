

using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.CreatorDtos;

public class MakeRequestResponseDto
{
    public string? SessionId { get; set; }
    public string? Message { get; set; }
    public string? Type { get; set; } = default!;
    public string? Script { get; set; } = default!;
    public string? UserId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public RequestStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? CreatorId { get; set; }
}
