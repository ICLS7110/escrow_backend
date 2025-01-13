using Escrow.Api.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Events.UserPanel;

namespace Escrow.Api.Application.UserPanel.Commands.CreateUser
{
    public record CreateUserCommand : IRequest<int>
    {
        public int UserId { get; init; }
        public string? FullName { get; set; }
        public string? EmailAddress { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        // Business Fields
        public string? BusinessManagerName { get; set; }
        public string? BusinessEmail { get; set; }
        public string? VatId { get; set; }
        //public byte[]? ProofOfBusiness { get; set; } // File as binary

        // Bank Account Details Fields
        public string? AccountHolderName { get; set; }
        public string? IBANNumber { get; set; }
        public string? BICCode { get; set; }

        public string? LoginMethod { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateUserCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var entity = new UserDetail
            {
                UserId = request.UserId,
                FullName = request.FullName,
                EmailAddress = request.EmailAddress,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                BusinessManagerName = request.BusinessManagerName,
                BusinessEmail = request.BusinessEmail,
                VatId = request.VatId,
                //ProofOfBusiness = request.ProofOfBusiness,
                AccountHolderName = request.AccountHolderName,
                IBANNumber = request.IBANNumber,
                BICCode = request.BICCode,
                LoginMethod = request.LoginMethod
            };

            entity.AddDomainEvent(new UserCreatedEvent(entity));
            _context.UserDetails.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }
}
