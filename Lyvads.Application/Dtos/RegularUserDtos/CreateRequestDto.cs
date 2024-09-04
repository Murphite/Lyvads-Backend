
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

public class PaymentDTO
{
    public string? ProductName { get; set; }
    public int Amount { get; set; }
    public string? ReturnUrl { get; set; }
    public PaymentMethod? Method { get; set; } // Card, Wallet, Online
    public string? UserId { get; set; }
}