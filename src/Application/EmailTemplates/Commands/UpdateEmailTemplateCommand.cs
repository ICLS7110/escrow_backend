using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Escrow.Api.Application.Common.Constants;

namespace Escrow.Api.Application.EmailTemplates.Commands;

public record UpdateEmailTemplateCommand : IRequest<Result<object>>
{
    public int TemplateId { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

public class UpdateEmailTemplateCommandHandler : IRequestHandler<UpdateEmailTemplateCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateEmailTemplateCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<object>> Handle(UpdateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var template = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template == null)
        {
            return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("EmailTemplateNotFound", language));
        }

        template.Subject = request.Subject;
        template.Body = request.Body;
        template.Name = request.Name;

        _context.EmailTemplates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("EmailTemplateUpdatedSuccessfully", language), null);
    }
}



































//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using MediatR;

//namespace Escrow.Api.Application.EmailTemplates.Commands;

//public record UpdateEmailTemplateCommand : IRequest<Result<object>>
//{
//    public int TemplateId { get; init; }
//    public string Subject { get; init; } = string.Empty;
//    public string Body { get; init; } = string.Empty;
//    public string Name { get; init; } = string.Empty;
//}

//public class UpdateEmailTemplateCommandHandler : IRequestHandler<UpdateEmailTemplateCommand, Result<object>>
//{
//    private readonly IApplicationDbContext _context;

//    public UpdateEmailTemplateCommandHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Result<object>> Handle(UpdateEmailTemplateCommand request, CancellationToken cancellationToken)
//    {
//        var template = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

//        if (template == null)
//        {
//            return Result<object>.Failure(StatusCodes.Status404NotFound, "Email Template not found.");
//        }

//        template.Subject = request.Subject;
//        template.Body = request.Body;
//        template.Name = request.Name;

//        _context.EmailTemplates.Update(template);
//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<object>.Success(StatusCodes.Status200OK, "Email Template updated successfully.", null);
//    }
//}
