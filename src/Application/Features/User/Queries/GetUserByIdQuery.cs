namespace Escrow.Api.Application.Features.Queries;

using Escrow.Api.Application.DTOs;
public record GetUserByIdQuery : IRequest<Result<UserDto>>
{
    public required string Id { get; set; }
}
