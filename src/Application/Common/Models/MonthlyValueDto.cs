using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Applcation.Common.Models;

namespace Escrow.Api.Application.Common.Models;
//public class MonthlyValueDto
//{
//    public string? Key { get; set; }  // Represents the period label (e.g., "Week of 01 Jan", "Q1 2023", "Jan 2023")
//    public decimal? Value { get; set; } // Represents the total value (e.g., total commission or escrow tax)

//}



public class MonthlyValueDto
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> Values { get; set; } = new();
}
