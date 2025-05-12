using Escrow.Api.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Events.UserPanel;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.UserPanel.Commands.CreateUser
{
    public record CreateUserCommand : IRequest<int>
    {        
        public string UserId { get; set; }= string.Empty;
        public string? FullName { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        // Business Fields
        public string? BusinessManagerName { get; set; }
        public string? BusinessEmail { get; set; }
        public string? VatId { get; set; }
        //public byte[]? ProofOfBusiness { get; set; } // File as binary
        public string? LoginMethod { get; set; }
        public string? BusinessProof { get; set; }
        public string? CompanyEmail { get; set; }
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
            var isExists=_context.UserDetails.Any(x => x.BusinessEmail == request.BusinessEmail );
            if (isExists) 
            {
                throw new ValidationException("The business email address already registed with weLink.com");
            }
            var entity = new UserDetail
            {      
                UserId = request.UserId,
                FullName = request.FullName,
                EmailAddress = request.EmailAddress,
                PhoneNumber = request.PhoneNumber,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                BusinessManagerName = request.BusinessManagerName,
                BusinessEmail = request.BusinessEmail,
                VatId = request.VatId,               
                LoginMethod = request.LoginMethod,
                BusinessProof= request.BusinessProof,
                CompanyEmail = request.CompanyEmail,
                Role = nameof(Roles.User),
            };

            
            _context.UserDetails.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }
}
