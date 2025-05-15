using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.EmailTemplate;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.EmailTemplates.Queries;

// ✅ Using `record` for query request, keeping it immutable and structured
public record GetEmailTemplateByIdQuery(int Id) : IRequest<Result<EmailTemplateDTO>>;

public class GetEmailTemplateByIdQueryHandler : IRequestHandler<GetEmailTemplateByIdQuery, Result<EmailTemplateDTO>>
{
    private readonly IApplicationDbContext _context;

    public GetEmailTemplateByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Result<EmailTemplateDTO>> Handle(GetEmailTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _context.EmailTemplates
            .Where(t => t.Id == request.Id)
            .Select(t => new EmailTemplateDTO
            {
                Id = t.Id,
                Name = t.Name,
                Subject = t.Subject,
                Body = t.Body,
                IsActive = t.IsActive,
            })
            .FirstOrDefaultAsync(cancellationToken);

        return template != null
            ? Result<EmailTemplateDTO>.Success(StatusCodes.Status200OK, "Email template retrieved successfully.", template)
            : Result<EmailTemplateDTO>.Failure(StatusCodes.Status404NotFound, "Email template not found.");
    }
}
