using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models;
public class MonthlyValueDto
{
    public string Month { get; set; } = string.Empty; // e.g., "Apr 2025"
    public decimal Value { get; set; }                // e.g., 1250.00
}
