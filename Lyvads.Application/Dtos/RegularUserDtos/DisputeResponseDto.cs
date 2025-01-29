
using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.RegularUserDtos;

public class DisputeResponseDto
{
    public string RequestId { get; set; } = default!;
    public string Reason { get; set; } = default!;
    public string DisputeMessage { get; set; } = default!;
    public string? Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string RegularUserFullName { get; set; } = default!;
    public string CreatorFullName { get; set; } = default!;
    public decimal Amount { get; set; }
}

public class OpenDisputeDto
{
    public List<string>? DisputeReason { get; set; }
    public string? Message { get; set; }
}
