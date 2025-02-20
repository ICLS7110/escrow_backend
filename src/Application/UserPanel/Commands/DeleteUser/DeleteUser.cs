using Ardalis.GuardClauses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Events.UserPanel;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.UserPanel.Commands.DeleteUser
{
    public record DeleteUserCommand(int Id) : IRequest;
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        public DeleteUserCommandHandler(IApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.UserDetails
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new EscrowDataNotFoundException("User Details Not Found.");
            }

            entity.RecordState = RecordState.Deleted;
            entity.DeletedAt = DateTimeOffset.UtcNow;
            entity.DeletedBy = _jwtService.GetUserId().ToInt();
            _context.UserDetails.Update(entity);           

            await _context.SaveChangesAsync(cancellationToken);
        }

    }
}
