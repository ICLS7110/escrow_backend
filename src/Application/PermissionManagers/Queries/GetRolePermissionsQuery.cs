using Escrow.Api.Application.Common.Models.AssignPermissionDtos;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Interfaces;

public record GetRoleMenuPermissionsQuery : IRequest<PaginatedList<RoleMenuPermissionDto>>
{
    public int? UserId { get; init; }   // Nullable UserId for filtering
    public int? PageNumber { get; init; } = 1; // Nullable page number
    public int? PageSize { get; init; } = 10;  // Nullable page size
}


public class GetRoleMenuPermissionsQueryHandler : IRequestHandler<GetRoleMenuPermissionsQuery, PaginatedList<RoleMenuPermissionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRoleMenuPermissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<RoleMenuPermissionDto>> Handle(GetRoleMenuPermissionsQuery request, CancellationToken cancellationToken)
    {
        // Ensure nullable values are converted or defaulted to non-nullable types
        int pageNumber = request.PageNumber ?? 1; // Default to 1 if null
        int pageSize = request.PageSize ?? 10;    // Default to 10 if null

        // Fetch the RoleMenuPermissions data from the database (no grouping here)
        var query = _context.RoleMenuPermissions.AsQueryable();



        if (request.UserId.HasValue)
        {
            query = query.Where(r => r.UserId == request.UserId.Value);
        }




        // Fetch data with required filtering (add any filtering logic here if needed)
        var result = await query.ToListAsync(cancellationToken);

        // Group by UserId and MenuId in memory
        var queryResult = result
            .GroupBy(r => r.UserId) // Group by UserId
            .Select(g => new RoleMenuPermissionDto
            {
                UserId = g.Key ?? 0, // Handle nullable UserId, default to 0 if null
                MenuPermissions = g.GroupBy(r => r.MenuId) // Group by MenuId
                                   .Select(mg => new MenuPermissionDto
                                   {
                                       MenuId = mg.Key, // Handle nullable MenuId, default to 0 if null
                                       PermissionIds = mg.Select(r => r.PermissionId).ToList() // Handle nullable PermissionId, default to 0 if null
                                   }).ToList()
            })
            .OrderBy(r => r.UserId)
            .ToList();

        // Apply pagination to the result
        var paginatedData = queryResult.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        var totalCount = queryResult.Count();

        return new PaginatedList<RoleMenuPermissionDto>(paginatedData, totalCount, pageNumber, pageSize);
    }

}

