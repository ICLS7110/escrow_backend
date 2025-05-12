using Escrow.Api.Application;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml.FormulaParsing.Utilities;

public record UpdateDetailsCommand : IRequest<Result<string>>
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
}

public class UpdateDetailsCommandHandler : IRequestHandler<UpdateDetailsCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public UpdateDetailsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(UpdateDetailsCommand request, CancellationToken cancellationToken)
    {
        var adminUser = await _context.UserDetails.FindAsync(new object[] { request.Id }, cancellationToken);

        if (adminUser == null)
        {
            return Result<string>.Failure(StatusCodes.Status404NotFound, "Admin Detail Not found.");
        }

        // If the provided role matches the existing role, proceed with updating details
        if (adminUser.Role == request.Role)
        {
            adminUser.FullName = request.UserName;
            // If role is NOT "Sub-Admin", update both Name and Email
            if (adminUser.Role.ToLower() == nameof(Roles.Admin).ToLower())
            {
                adminUser.EmailAddress = request.Email;
            }
            adminUser.ProfilePicture = request.Image;
            adminUser.LastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(StatusCodes.Status200OK, "Updated successfully.");
        }

        // If roles do not match, return an error
        return Result<string>.Failure(StatusCodes.Status400BadRequest, "Role mismatch. Cannot update user.");
    }
}
