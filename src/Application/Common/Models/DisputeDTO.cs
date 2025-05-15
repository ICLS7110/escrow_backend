using System;
using System.Collections.Generic;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Domain.Entities.Disputes;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.Common.Models;

public class DisputeDTO
{
    public int Id { get; set; }
    public DateTime DisputeDateTime { get; set; }
    public string RaisedBy { get; set; } = string.Empty;  // Buyer/Seller
    public string Status { get; set; } = string.Empty;  // Enum as String
    public string DisputeDoc { get; set; } = string.Empty;
    public string DisputeReason { get; set; } = string.Empty;
    public string DisputeDescription { get; set; } = string.Empty;
    public int? ArbitratorId { get; set; }  // Nullable Arbitrator ID
    public ContractDTO? ContractDetails { get; set; }  // ✅ Nullable contract details
}
