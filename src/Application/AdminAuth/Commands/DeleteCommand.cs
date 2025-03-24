using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.AdminAuth.Commands;

// Command to soft delete a sub-admin
public class DeleteCommand : IRequest<Result<bool>>
{
    public int Id { get; init; }
    public int DeletedBy { get; init; }
}

// Handler to handle the soft delete command
public class DeleteCommandHandler : IRequestHandler<DeleteCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteCommand request, CancellationToken cancellationToken)
    {
        var subAdminUser = await _context.AdminUsers
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (subAdminUser == null)
        {
            return Result<bool>.Failure(StatusCodes.Status404NotFound, "Not found.");
        }

        subAdminUser.IsDeleted = true; // Mark as deleted
        subAdminUser.DeletedAt = DateTime.UtcNow;
        subAdminUser.DeletedBy = request.DeletedBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(StatusCodes.Status200OK, "Deleted successfully.", true);
    }
}

