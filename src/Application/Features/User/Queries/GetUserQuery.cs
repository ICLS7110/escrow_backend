namespace Escrow.Api.Application.Features.Queries;

using Escrow.Api.Application.DTOs;
public record GetUserQuery : IRequest<Result<UserDto>>
{
}
