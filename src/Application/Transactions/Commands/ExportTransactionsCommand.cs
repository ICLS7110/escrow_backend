using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.Transactions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Escrow.Api.Application.Transactions.Commands;

public record ExportTransactionsCommand : IRequest<Result<string>>
{
    public string? Keyword { get; init; }
    //public List<int>? BuyerIds { get; init; }
    //public List<int>? SellerIds { get; init; }
    public string? TransactionType { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class ExportTransactionsCommandHandler : IRequestHandler<ExportTransactionsCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public ExportTransactionsCommandHandler(IApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Result<string>> Handle(ExportTransactionsCommand request, CancellationToken cancellationToken)
    {
        var query = _context.Transactions.AsQueryable();

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            query = query.Where(t => t.TransactionType != null && t.TransactionType.Contains(request.Keyword));
        }

        //if (request.BuyerIds != null && request.BuyerIds.Any())
        //{
        //    query = query.Where(t => request.BuyerIds.Contains(t.Id));
        //}

        //if (request.SellerIds != null && request.SellerIds.Any())
        //{
        //    query = query.Where(t => request.SellerIds.Contains(t.Id));
        //}

        if (!string.IsNullOrEmpty(request.TransactionType))
        {
            query = query.Where(t => t.TransactionType != null && t.TransactionType == request.TransactionType);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(t => t.TransactionDateTime >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(t => t.TransactionDateTime <= request.EndDate.Value);
        }

        var transactions = await query.ToListAsync(cancellationToken);

        if (transactions == null || !transactions.Any())
        {
            return Result<string>.Failure(StatusCodes.Status404NotFound, "No transactions found for export.");
        }

        var filePath = await GenerateExcelFile(transactions);
        return Result<string>.Success(StatusCodes.Status200OK, "Transactions exported successfully.", filePath);
    }

    private async Task<string> GenerateExcelFile(List<Transaction> transactions)
    {
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Exports");
        Directory.CreateDirectory(directoryPath);
        var filePath = Path.Combine(directoryPath, $"Transactions_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");

        try
        {
            // ✅ Set the license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Transactions");
                worksheet.Cells[1, 1].Value = "Transaction ID";
                worksheet.Cells[1, 2].Value = "Date";
                worksheet.Cells[1, 3].Value = "Amount";
                worksheet.Cells[1, 4].Value = "Type";
                worksheet.Cells[1, 5].Value = "From Payee";
                worksheet.Cells[1, 6].Value = "To Recipient";
                worksheet.Cells[1, 7].Value = "Contract ID";

                for (int i = 0; i < transactions.Count; i++)
                {
                    var transaction = transactions[i];
                    worksheet.Cells[i + 2, 1].Value = transaction.Id;
                    worksheet.Cells[i + 2, 2].Value = transaction.TransactionDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    worksheet.Cells[i + 2, 3].Value = transaction.TransactionAmount;
                    worksheet.Cells[i + 2, 4].Value = transaction.TransactionType ?? "N/A";
                    worksheet.Cells[i + 2, 5].Value = transaction.FromPayee ?? "N/A";
                    worksheet.Cells[i + 2, 6].Value = transaction.ToRecipient ?? "N/A";
                    worksheet.Cells[i + 2, 7].Value = transaction.ContractId?.ToString() ?? "N/A";
                }

                await File.WriteAllBytesAsync(filePath, package.GetAsByteArray());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating Excel: {ex.Message}");
            throw;
        }

        return filePath;
    }

}
