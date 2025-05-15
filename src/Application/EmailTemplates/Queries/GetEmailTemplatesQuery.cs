using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.EmailTemplate;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Escrow.Api.Application.Common.Constants;

namespace Escrow.Api.Application.EmailTemplates.Queries;

public class GetEmailTemplatesQuery : IRequest<Result<List<EmailTemplateDTO>>> { }

public class GetEmailTemplatesQueryHandler : IRequestHandler<GetEmailTemplatesQuery, Result<List<EmailTemplateDTO>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetEmailTemplatesQueryHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<List<EmailTemplateDTO>>> Handle(GetEmailTemplatesQuery request, CancellationToken cancellationToken)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var templates = await _context.EmailTemplates
            .Where(t => !t.IsDeleted)
            .OrderByDescending(t => t.LastModified)
            .Select(t => new EmailTemplateDTO
            {
                Id = t.Id,
                Name = t.Name,
                Subject = t.Subject,
                Body = t.Body,
                IsActive = t.IsActive,
                IsDeleted = t.IsDeleted,
            })
            .ToListAsync(cancellationToken);

        if (!templates.Any())
        {
            return Result<List<EmailTemplateDTO>>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("EmailTemplateNotFound", language));
        }

        return Result<List<EmailTemplateDTO>>.Success(StatusCodes.Status200OK, AppMessages.Get("EmailTemplateRetrievedSuccessfully", language), templates);
    }
}










































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models.EmailTemplate;
//using Escrow.Api.Application.DTOs;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using MediatR;

//namespace Escrow.Api.Application.EmailTemplates.Queries;
//public class GetEmailTemplatesQuery : IRequest<Result<List<EmailTemplateDTO>>>
//{
//}

//public class GetEmailTemplatesQueryHandler : IRequestHandler<GetEmailTemplatesQuery, Result<List<EmailTemplateDTO>>>
//{
//    private readonly IApplicationDbContext _context;

//    public GetEmailTemplatesQueryHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Result<List<EmailTemplateDTO>>> Handle(GetEmailTemplatesQuery request, CancellationToken cancellationToken)
//    {
//        var templates = await _context.EmailTemplates
//            .Where(t => !t.IsDeleted) // Exclude deleted templates
//            .OrderByDescending(t => t.LastModified) // Order by Last Modified Date
//            .Select(t => new EmailTemplateDTO
//            {
//                Id = t.Id,
//                Name = t.Name,
//                Subject = t.Subject,
//                Body = t.Body,
//                IsActive = t.IsActive,
//                IsDeleted = t.IsDeleted,
//            })
//            .ToListAsync(cancellationToken);

//        if (!templates.Any())
//        {
//            return Result<List<EmailTemplateDTO>>.Failure(StatusCodes.Status404NotFound, "No email templates found.");
//        }

//        return Result<List<EmailTemplateDTO>>.Success(StatusCodes.Status200OK, "Email templates retrieved successfully.", templates);
//    }
//}
