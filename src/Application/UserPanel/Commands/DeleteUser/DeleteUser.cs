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

namespace Escrow.Api.Application.UserPanel.Commands.DeleteUser
{
    public record DeleteUserCommand(int Id) : IRequest;
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteUserCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.UserDetails
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (entity == null)
            {
                throw new CustomValidationException("User Details Not Found.");
            }

            entity.RecordState = "Deleted";
            _context.UserDetails.Update(entity);

            

            await _context.SaveChangesAsync(cancellationToken);
        }

    }
}
