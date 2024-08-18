
using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos.RegularUserDtos;

public class CreateRequestDto
{
    public string UserId { get; set; } = default!;
    public string CreatorId { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Script { get; set; } = default!;
    public RequestType RequestType { get; set; }
    public decimal Amount { get; set; }
    public string? Source { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
}

public enum PaymentMethod
{
    Wallet,
    Online,
    ATMCard
}
