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
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.UserPanel.Commands.UpdateUser
{
    public record UpdateUserCommand : IRequest<Result<int>>
    {        
        
        public string? FullName { get; set; }
        public string? EmailAddress { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        // Business Fields
        public string? BusinessManagerName { get; set; }
        public string? BusinessEmail { get; set; }
        public string? VatId { get; set; }
        public string? BusinessProof { get; set; }
        public string? CompanyEmail { get; set; }
        public string? ProfilePicture { get; set; }

    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        public UpdateUserCommandHandler(IApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<Result<int>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.UserDetails
                .FindAsync(new object[] { _jwtService.GetUserId().ToInt() }, cancellationToken);

            if (entity == null) 
                return Result<int>.Failure(StatusCodes.Status404NotFound, "User Details Not Found.");
            
            entity.FullName = request.FullName;
            entity.EmailAddress = request.EmailAddress;
            entity.Gender = request.Gender;
            entity.DateOfBirth = request.DateOfBirth;
            entity.BusinessManagerName = request.BusinessManagerName;
            entity.BusinessEmail = request.BusinessEmail;
            entity.VatId = request.VatId;
            //entity.BusinessProof = request.BusinessProof;
            //entity.CompanyEmail = request.CompanyEmail;
            //entity.ProfilePicture = request.ProfilePicture;
            
            await _context.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(StatusCodes.Status200OK, "Success");
        }
    }
}
