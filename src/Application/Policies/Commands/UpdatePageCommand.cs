using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Policies.Commands;
public class UpdatePagesCommand : IRequest<Result<object>>
{
    public List<PageUpdateDto> Content { get; init; } = new();
}

public class PageUpdateDto
{
    public int? Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
}

public class UpdatePagesCommandHandler : IRequestHandler<UpdatePagesCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;

    public UpdatePagesCommandHandler(IApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Result<object>> Handle(UpdatePagesCommand request, CancellationToken cancellationToken)
    {
        if (request.Content == null || request.Content.Count == 0)
        {
            return Result<object>.Failure(StatusCodes.Status400BadRequest, "No pages provided for update.");
        }

        var pageIds = request.Content.Select(p => p.Id).Where(id => id.HasValue).Cast<int>().ToList();

        if (!pageIds.Any())
        {
            return Result<object>.Failure(StatusCodes.Status400BadRequest, "No valid page IDs provided.");
        }

        var pagesToUpdate = await _context.Pages
            .Where(x => pageIds.Contains(x.Id) && (x.IsDeleted == null || !x.IsDeleted.Value))
            .AsTracking()
            .ToListAsync(cancellationToken);

        if (pagesToUpdate.Count == 0)
        {
            return Result<object>.Failure(StatusCodes.Status404NotFound, "No matching pages found for update.");
        }

        foreach (var page in pagesToUpdate)
        {
            var updateData = request.Content.FirstOrDefault(p => p.Id == page.Id);
            if (updateData != null)
            {
                page.Title = string.IsNullOrWhiteSpace(updateData.Name) ? page.Title : updateData.Name;
                page.Content = string.IsNullOrWhiteSpace(updateData.Description) ? page.Content : updateData.Description;
                page.IsActive = true;
                page.LastModified = DateTime.UtcNow;
                page.LastModifiedBy = "System"; // Replace with actual user ID
            }
        }

        Console.WriteLine("Saving changes...");
        await _context.SaveChangesAsync(cancellationToken);
        Console.WriteLine("Changes saved successfully.");

        return Result<object>.Success(StatusCodes.Status200OK, "Pages updated successfully.", new { });
    }
}
