
using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.RegularUserDtos;

public class DisputeResponseDto
{
    public string RequestId { get; set; } = default!;
    public DisputeReasons Reason { get; set; } = default!;
    public string DisputeMessage { get; set; } = default!;
    public DisputeStatus Status { get; set; } = DisputeStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; }
    public string RegularUserFullName { get; set; } = default!;
    public string CreatorFullName { get; set; } = default!;
    public decimal Amount { get; set; }
}


public class OpenDisputeDto
{
    public string Message { get; set; } = default!;
    public DisputeReasons DisputeReason { get; set; }
}
