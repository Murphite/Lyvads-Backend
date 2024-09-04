
namespace Lyvads.Application.Dtos.WalletDtos;

public class BankDto
{

}
public class BankTransferRequest
{
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public required string UserId { get; set; }
    public required string Reference { get; set; }
    public required string BankName { get; set; }
    public required string AccountNumber { get; set; }
    public required string AccountHolderName { get; set; }
}

public class BankTransferResponse
{
    public bool IsSuccess { get; set; }
    public string? TransferInstructions { get; set; }
}
