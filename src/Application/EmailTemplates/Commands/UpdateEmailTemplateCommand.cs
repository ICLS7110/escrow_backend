using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.EmailTemplates.Commands;
public record UpdateEmailTemplateCommand : IRequest<Result<int>>
{
    public int TemplateId { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
}

public class UpdateEmailTemplateCommandHandler : IRequestHandler<UpdateEmailTemplateCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmailTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(UpdateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template == null)
        {
            return Result<int>.Failure(StatusCodes.Status404NotFound, "Email Template not found.");
        }

        template.Subject = request.Subject;
        template.Body = request.Body;

        _context.EmailTemplates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(StatusCodes.Status200OK, "Email Template updated successfully.", template.Id);
    }
}

