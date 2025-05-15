using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.AML;
public class AMLSettingsDto
{
    
    public int AMLSettingsId { get; set; } // Threshold for high-value transactions
    public decimal HighAmountLimit { get; set; } // Threshold for high-value transactions
    public string FrequencyThreshold { get; set; } = string.Empty; // Time interval for monitoring (Hourly, Daily, etc.)
    public decimal BenchmarkLimit { get; set; } // Benchmark limit for pausing transactions
}
