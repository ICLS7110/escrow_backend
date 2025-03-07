using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.AdminPanel;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Application.DTOs;

namespace Escrow.Api.Application.AdminAuth.Commands
{
    public record AdminForgotPasswordCommand : IRequest<Result<object>> // Changed Result<string> to Result<object>
    {
        public string Email { get; init; } = string.Empty;
    }

    public class AdminForgotPasswordCommandHandler : IRequestHandler<AdminForgotPasswordCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public AdminForgotPasswordCommandHandler(
            IApplicationDbContext context,
            IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<Result<object>> Handle(AdminForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            // Check if the email exists
            var adminUser = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (adminUser == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, "Email not found.");
            }

            string otp = "1234";//new Random().Next(100000, 999999).ToString();

            adminUser.OTP = otp;
            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, "OTP sent successfully.", new { });
        }
    }
}













//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Domain.Entities.AdminPanel;
//using Escrow.Api.Domain.Entities.UserPanel;

//namespace Escrow.Api.Application.AdminAuth.Commands;


//public record AdminForgotPasswordCommand : IRequest<int>
//{
//    public string Email { get; set; } = string.Empty;
//}

//public class AdminForgotPasswordCommandHandler : IRequestHandler<AdminForgotPasswordCommand, int>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IAESService _AESService;
//    private readonly IJwtService _jwtService;

//    public AdminForgotPasswordCommandHandler(IApplicationDbContext context, IAESService aESService, IJwtService jwtService)
//    {
//        _context = context;
//        _AESService = aESService;
//        _jwtService = jwtService;
//    }

//    public async Task<int> Handle(AdminForgotPasswordCommand request, CancellationToken cancellationToken)
//    {


//        var entity = new AdminUser
//        {
//            Email = request.Email,
//        };
//        //{
//        //    UserDetailId = _jwtService.GetUserId().ToInt(),
//        //    AccountHolderName = request.AccountHolderName,
//        //    IBANNumber = _AESService.Encrypt(request.IBANNumber),
//        //    BankName = _AESService.Encrypt(request.BankName),
//        //    BICCode = request.BICCode,
//        //};

//        _context.AdminUsers.Add(entity);
//        await _context.SaveChangesAsync(cancellationToken);
//        int userId = _jwtService.GetUserId().ToInt();
//        var userentity = await _context.UserDetails.FindAsync(new object[] { userId }, cancellationToken);
//        if (userentity != null)
//        {
//            userentity.IsProfileCompleted = true;
//            await _context.SaveChangesAsync(cancellationToken);
//        }
//        return entity.Id;
//    }
//}

