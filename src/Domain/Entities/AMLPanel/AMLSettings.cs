using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.AMLPanel;
public class AMLSettings : BaseAuditableEntity
{

    public decimal HighAmountLimit { get; set; } = 10000; // Example threshold

    public string FrequencyThreshold { get; set; } = "Daily"; // Hourly, Daily, Weekly, Monthly

    public decimal BenchmarkLimit { get; set; } = 5000;

}
