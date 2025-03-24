using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using global::Escrow.Api.Application.DTOs;

namespace Escrow.Api.Application.ContractPanel.ContractQueries;
public class GetContractByIdQuery : IRequest<Result<object>>
{
    public int ContractId { get; init; }

    public GetContractByIdQuery(int contractId)
    {
        ContractId = contractId;
    }
}

public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _context;

    public GetContractByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object>> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
    {
        var contract = await _context.ContractDetails
            .Where(c => c.Id == request.ContractId)
            .Select(c => new
            {
                c.Id,
                c.ContractTitle,
                c.ServiceType,
                c.ServiceDescription,
                c.AdditionalNote,
                c.FeesPaidBy,
                c.FeeAmount,
                c.EscrowTax,
                c.TaxAmount,
                c.Status,
                c.StatusReason,
                c.Created,
                c.LastModified,
                Buyer = new
                {
                    c.BuyerName,
                    c.BuyerMobile
                },
                Seller = new
                {
                    c.SellerName,
                    c.SellerMobile
                },
                Invitation = _context.SellerBuyerInvitations
                    .Where(i => i.ContractId == c.Id)
                    .Select(i => new
                    {
                        i.Id,
                        i.InvitationLink,
                        i.Status,
                        i.Created,
                        i.LastModified
                    }).FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (contract == null)
        {
            return Result<object>.Failure(StatusCodes.Status404NotFound, "Contract not found.");
        }

        return Result<object>.Success(StatusCodes.Status200OK, "Contract retrieved successfully.", contract);
    }
}
