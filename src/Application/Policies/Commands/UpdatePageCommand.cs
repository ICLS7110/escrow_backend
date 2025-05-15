using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

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
    public string? Ar_Description { get; init; }
}

public class UpdatePagesCommandHandler : IRequestHandler<UpdatePagesCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdatePagesCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<Result<object>> Handle(UpdatePagesCommand request, CancellationToken cancellationToken)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        if (request.Content == null || request.Content.Count == 0)
        {
            return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("NoPagesProvided", language));
        }

        var pageIds = request.Content.Select(p => p.Id).Where(id => id.HasValue).Cast<int>().ToList();

        if (!pageIds.Any())
        {
            return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("NoValidPageIds", language));
        }

        var pagesToUpdate = await _context.Pages
            .Where(x => pageIds.Contains(x.Id) && (x.IsDeleted == null || !x.IsDeleted.Value))
            .AsTracking()
            .ToListAsync(cancellationToken);

        if (pagesToUpdate.Count == 0)
        {
            return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("NoMatchingPagesFound", language));
        }

        foreach (var page in pagesToUpdate)
        {
            var updateData = request.Content.FirstOrDefault(p => p.Id == page.Id);
            if (updateData != null)
            {
                page.Title = string.IsNullOrWhiteSpace(updateData.Name) ? page.Title : updateData.Name;
                page.Content = string.IsNullOrWhiteSpace(updateData.Description) ? page.Content : updateData.Description;
                page.Ar_Description = string.IsNullOrWhiteSpace(updateData.Ar_Description) ? page.Ar_Description : updateData.Ar_Description;
                page.IsActive = true;
                page.LastModified = DateTime.UtcNow;
                page.LastModifiedBy = "System"; // TODO: Replace with actual user ID or user name
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("PagesUpdatedSuccessfully", language), new { });
    }
}













































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.Policies.Commands;
//public class UpdatePagesCommand : IRequest<Result<object>>
//{
//    public List<PageUpdateDto> Content { get; init; } = new();
//}

//public class PageUpdateDto
//{
//    public int? Id { get; init; }
//    public string? Name { get; init; }
//    public string? Description { get; init; }
//    public string? Ar_Description { get; init; }
//}

//public class UpdatePagesCommandHandler : IRequestHandler<UpdatePagesCommand, Result<object>>
//{
//    private readonly IApplicationDbContext _context;

//    public UpdatePagesCommandHandler(IApplicationDbContext context)
//    {
//        _context = context ?? throw new ArgumentNullException(nameof(context));
//    }

//    public async Task<Result<object>> Handle(UpdatePagesCommand request, CancellationToken cancellationToken)
//    {
//        if (request.Content == null || request.Content.Count == 0)
//        {
//            return Result<object>.Failure(StatusCodes.Status400BadRequest, "No pages provided for update.");
//        }

//        var pageIds = request.Content.Select(p => p.Id).Where(id => id.HasValue).Cast<int>().ToList();

//        if (!pageIds.Any())
//        {
//            return Result<object>.Failure(StatusCodes.Status400BadRequest, "No valid page IDs provided.");
//        }

//        var pagesToUpdate = await _context.Pages
//            .Where(x => pageIds.Contains(x.Id) && (x.IsDeleted == null || !x.IsDeleted.Value))
//            .AsTracking()
//            .ToListAsync(cancellationToken);

//        if (pagesToUpdate.Count == 0)
//        {
//            return Result<object>.Failure(StatusCodes.Status404NotFound, "No matching pages found for update.");
//        }

//        foreach (var page in pagesToUpdate)
//        {
//            var updateData = request.Content.FirstOrDefault(p => p.Id == page.Id);
//            if (updateData != null)
//            {
//                page.Title = string.IsNullOrWhiteSpace(updateData.Name) ? page.Title : updateData.Name;
//                page.Content = string.IsNullOrWhiteSpace(updateData.Description) ? page.Content : updateData.Description;
//                page.Ar_Description = string.IsNullOrWhiteSpace(updateData.Ar_Description) ? page.Ar_Description : updateData.Ar_Description;
//                page.IsActive = true;
//                page.LastModified = DateTime.UtcNow;
//                page.LastModifiedBy = "System"; // Replace with actual user ID
//            }
//        }

//        Console.WriteLine("Saving changes...");
//        await _context.SaveChangesAsync(cancellationToken);
//        Console.WriteLine("Changes saved successfully.");

//        return Result<object>.Success(StatusCodes.Status200OK, "Pages updated successfully.", new { });
//    }
//}
