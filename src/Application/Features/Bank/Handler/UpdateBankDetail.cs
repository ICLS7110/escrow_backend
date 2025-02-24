namespace Escrow.Api.Application.Features.Handler;

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Features.Commands;

public class UpdateBankHandler : IRequestHandler<UpdateBankCommand, Result<BankDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    private readonly IAESService _aesService;

    public UpdateBankHandler(IApplicationDbContext context, IMapper mapper, IJwtService jwtService, IAESService aesService)
    {
        _context = context;
        _mapper = mapper;
        _jwtService = jwtService;
        _aesService = aesService;
    }

    public async Task<Result<BankDto>> Handle(UpdateBankCommand request, CancellationToken cancellationToken)
    {
        int userId = int.Parse(_jwtService.GetUserId());

        var entity = await _context.BankDetails
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserDetailId == userId, cancellationToken);

        if (entity == null)
        {
            return Result<BankDto>.Failure(404, "Bank details not found.");
        }


        entity.AccountHolderName = request.AccountHolderName;
        entity.IBANNumber = _aesService.Encrypt(request.IBANNumber);
        entity.BankName = _aesService.Encrypt(request.BankName);
 
        entity.LastModified = DateTimeOffset.UtcNow;
        entity.LastModifiedBy = userId.ToString();

        await _context.SaveChangesAsync(cancellationToken);

        var bankDto = _mapper.Map<BankDto>(entity);
        return Result<BankDto>.Success(200, "Bank details updated successfully.", bankDto);
    }
}
