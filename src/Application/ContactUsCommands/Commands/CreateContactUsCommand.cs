using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.BankDetails.Commands;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.ContactUs;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.ContactUsCommands.Commands;
public  class CreateContactUsCommand : IRequest<int>
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateContactUsCommandHandler : IRequestHandler<CreateContactUsCommand, int>
{
    private readonly IApplicationDbContext _context;
    public CreateContactUsCommandHandler(IApplicationDbContext applicationDbContext)
    {
            _context = applicationDbContext;
    }

    public async Task<int> Handle(CreateContactUsCommand request, CancellationToken cancellationToken)
    {
        var entity = new ContactUs
        {
            FullName= request.FullName,
            Email= request.Email,
            Description= request.Description
            ,Created=DateTime.UtcNow
        };

        await _context.ContactUs.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }
}


