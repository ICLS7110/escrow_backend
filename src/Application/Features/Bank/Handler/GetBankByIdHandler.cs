namespace Escrow.Api.Application.Features.Handler;

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Features.Queries;

public class GetBankByIdHandler : IRequestHandler<GetBankByIdQuery, Result<BankDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAESService _AESService;
    private readonly IMapper _mapper;

    public GetBankByIdHandler(IApplicationDbContext context, IMapper mapper, IJwtService jwtService, IAESService aESService)
    {
        _context = context;
        _AESService = aESService;
        _mapper = mapper;
    }

    public async Task<Result<BankDto>> Handle(GetBankByIdQuery request, CancellationToken cancellationToken)
    {
        if (!int.TryParse(request.Id, out int id))
        {
            return Result<BankDto>.Failure(400, "Bad Request: Invalid ID format");
        }

        var result = await _context.BankDetails
            .AsNoTracking()
            .Where(x => x.UserDetailId == id)
            .ProjectTo<BankDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            return Result<BankDto>.Failure(404, "Bank details not found");
        }

        return Result<BankDto>.Success(200, "Bank details retrieved successfully", result);

    }
}
