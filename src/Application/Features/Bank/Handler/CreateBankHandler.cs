namespace Escrow.Api.Application.Handler; 

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Features.Commands;
using Escrow.Api.Domain.Entities;

public class CreateBankHandler : IRequestHandler<CreateBankCommand, Result<BankDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAESService _AESService;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public CreateBankHandler(IApplicationDbContext context, IAESService aESService, IJwtService jwtService, IMapper mapper)
    {
        _context = context;
        _AESService = aESService;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<Result<BankDto>> Handle(CreateBankCommand request, CancellationToken cancellationToken)
    {
        int userId = int.Parse(_jwtService.GetUserId()); 

        var entity = new Bank
        {
            UserDetailId = userId,
            AccountHolderName = request.AccountHolderName,
            IBANNumber = _AESService.Encrypt(request.IBANNumber),
            BankName = _AESService.Encrypt(request.BankName),
            BICCode = request.BICCode,
        };

        _context.BankDetails.Add(entity);

        var userEntity = await _context.UserDetails.FindAsync(new object[] { userId }, cancellationToken);
        if (userEntity != null)
        {
            userEntity.IsProfileCompleted = true;
        }

        await _context.SaveChangesAsync(cancellationToken); 

        var bankDto = _mapper.Map<BankDto>(entity);
        return Result<BankDto>.Success(201, "Bank details created successfully.", bankDto);
    }
}
