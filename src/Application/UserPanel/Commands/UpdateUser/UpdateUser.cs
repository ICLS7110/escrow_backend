using Ardalis.GuardClauses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Events.UserPanel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Escrow.Api.Application.UserPanel.Commands.UpdateUser
{
    public record UpdateUserCommand : IRequest
    {
        public int Id { get; init; }
        public string UserId { get; init; } = string.Empty;
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

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateUserCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.UserDetails
                .FindAsync(new object[] { request.UserId }, cancellationToken);

            Guard.Against.NotFound(request.UserId, entity);

            entity.UserId = request.UserId;
            entity.FullName = request.FullName;
            entity.EmailAddress = request.EmailAddress;
            entity.Gender = request.Gender;
            entity.DateOfBirth = request.DateOfBirth;
            entity.BusinessManagerName = request.BusinessManagerName;
            entity.BusinessEmail = request.BusinessEmail;
            entity.VatId = request.VatId;
            //entity.ProofOfBusiness = request.ProofOfBusiness;
            entity.AccountHolderName = request.AccountHolderName;
            entity.IBANNumber = request.IBANNumber;
            entity.BICCode = request.BICCode;
            entity.LoginMethod = request.LoginMethod;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
