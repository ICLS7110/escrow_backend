using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Application.Common.Models;
public class SearchTransactionsRequestDTO
{
    [FromQuery] public string? Keyword { get; set; }

    //[FromQuery(Name = "buyer_ids")]
    //public string? BuyerIdsRaw { get; set; }

    //[FromQuery(Name = "seller_ids")]
    //public string? SellerIdsRaw { get; set; }

    [FromQuery] public string? TransactionType { get; set; }
    [FromQuery] public DateTime? StartDate { get; set; }
    [FromQuery] public DateTime? EndDate { get; set; }

    [FromQuery] public int PageNumber { get; set; } = 1;  // ✅ Default to page 1
    [FromQuery] public int PageSize { get; set; } = 10;   // ✅ Default page size 10

    //public List<int> BuyerIds => ParseIds(BuyerIdsRaw);
    //public List<int> SellerIds => ParseIds(SellerIdsRaw);

    private static List<int> ParseIds(string? idsRaw)
    {
        return string.IsNullOrEmpty(idsRaw)
            ? new List<int>()
            : idsRaw.Split(',').Select(int.Parse).ToList();
    }
}
