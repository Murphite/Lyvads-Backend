namespace Lyvads.API.Presentation.Dtos;

public class WithdrawRequestDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
}