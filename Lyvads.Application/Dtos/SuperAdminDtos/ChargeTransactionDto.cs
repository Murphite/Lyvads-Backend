﻿
using Lyvads.Domain.Enums;

namespace Lyvads.Application.Dtos;

public class ChargeSummaryDto
{
    public decimal TotalCharges { get; set; } 
    public List<ChargeTypeTotal> ChargeTypeTotals { get; set; } = new List<ChargeTypeTotal>();
}

public class ChargeTypeTotal
{
    public string ChargeName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; } 
}


public class ChargeTransactionDto
{
    public string? UserName { get; set; }
    public string? Id { get; set; }
    public string? ChargeName { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset DateCharged { get; set; }
    public string? Status { get; set; }
}

public class CreateChargeDto
{
    public string? ChargeName { get; set; }
    public decimal Percentage { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
}

public class CreateChargeResponse
{
    public string? Id { get; set; }
    public string? ChargeName { get; set; }
    public decimal Percentage { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
}

public class EditChargeDto
{
    public string? ChargeName { get; set; }
    public decimal Percentage { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public ChargeStatus Status { get; set; } 
}

public class EditChargeResponse
{
    public string? Id { get; set; }
    public string? ChargeName { get; set; }
    public decimal Percentage { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public string? Status { get; set; } 
}

public class ChargeDto
{
    public string? Id { get; set; }
    public string? ChargeName { get; set; }
    public decimal Percentage { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public string? Status { get; set; } 
}



