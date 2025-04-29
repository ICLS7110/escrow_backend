using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.AdminPanel;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Infrastructure.Security;
using MediatR;

namespace Escrow.Api.Application.Admin.Commands;

public record CreateSubAdminCommand : IRequest<int>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CreateSubAdminCommandHandler : IRequestHandler<CreateSubAdminCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IAESService _AESService;
    private readonly IJwtService _jwtService;

    public CreateSubAdminCommandHandler(IApplicationDbContext context, IAESService aESService, IJwtService jwtService)
    {
        _context = context;
        _AESService = aESService;
        _jwtService = jwtService;
    }

    public async Task<int> Handle(CreateSubAdminCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _context.UserDetails.FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new Exception("Email already exists.");
        }

        var entity = new UserDetail
        {
            UserId = Guid.NewGuid().ToString(),
            FullName = request.Username,
            EmailAddress = request.Email,
            Password = request.Password,
            Role = request.Role,
            IsActive = true,
            Created = DateTime.UtcNow,
            CreatedBy = _jwtService.GetUserId().ToString()
        };

        _context.UserDetails.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
