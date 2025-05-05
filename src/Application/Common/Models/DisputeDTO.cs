using System;
using System.Collections.Generic;
using Escrow.Api.Domain.Entities.Disputes;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.Common.Models;

public class DisputeDTO
{
    public int Id { get; set; }
    public DateTime DisputeDateTime { get; set; }
    public string RaisedBy { get; set; } = string.Empty;  // Buyer/Seller
    public string Status { get; set; } = string.Empty;  // Enum as String
    public decimal EscrowAmount { get; set; }
    public decimal ContractAmount { get; set; }
    public decimal FeesTaxes { get; set; }
    public string DisputeDoc { get; set; } = string.Empty;
    public List<string> Messages { get; set; } = new();  // ✅ Extract messages as strings
    public int? ArbitratorId { get; set; }  // Nullable Arbitrator ID
    public string? AdminDecision { get; set; }  // Admin's final decision
    public decimal? ReleaseAmount { get; set; }  // Amount released
    public string? ReleaseTo { get; set; }  // Buyer/Seller receiving the release
    public string? ContractDetails { get; set; }  // ✅ Nullable contract details
}
