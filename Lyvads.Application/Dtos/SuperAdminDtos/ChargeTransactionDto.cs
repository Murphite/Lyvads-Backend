
using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos;

public class ChargeTransactionDto
{
    public string? UserName { get; set; }
    public ChargeReason ChargeName { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset DateCharged { get; set; }
    public CTransStatus Status { get; set; }
}

public class CreateChargeDto
{
    public ChargeReason ChargeName { get; set; }
    public decimal Percentage { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
}

public class EditChargeDto
{
    public ChargeReason ChargeName { get; set; }
    public decimal Percentage { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public ChargeStatus Status { get; set; } // e.g., Active, Inactive
}

public class ChargeDto
{
    public ChargeReason ChargeName { get; set; }
    public decimal Percentage { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public ChargeStatus Status { get; set; } // e.g., Active, Inactive
}

