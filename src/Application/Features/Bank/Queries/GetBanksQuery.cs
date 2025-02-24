
namespace Escrow.Api.Application.Features.Queries;

using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;

public record GetBanksQuery : IRequest<Result<PaginatedList<BankDto>>>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}
