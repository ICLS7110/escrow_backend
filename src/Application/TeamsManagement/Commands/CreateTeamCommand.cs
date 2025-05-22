using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.TeamsManagement.Commands;
using Escrow.Api.Domain.Constants;
using Escrow.Api.Domain.Entities.TeamMembers;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.TeamsManagement.Commands
{
    public class CreateTeamCommand : IRequest<Result<object>>
    {
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? RoleType { get; set; }
        //public string? ContractId { get; set; }
        public List<string> ContractId { get; set; } = new();
    }

    public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateTeamCommandHandler(
            IApplicationDbContext context,
            IJwtService jwtService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<Result<object>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
        {
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("TeamNameRequired", language));

            var userIdFromJwt = _jwtService.GetUserId();
            if (!int.TryParse(userIdFromJwt, out int createdBy) || createdBy == 0)
                return Result<object>.Failure(StatusCodes.Status401Unauthorized, AppMessages.Get("Unauthorized", language));

            if (string.IsNullOrWhiteSpace(request.RoleType))
                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("RoleTypeRequired", language));

            string? contractIdCsv = request.ContractId != null ? string.Join(",", request.ContractId) : string.Empty;

            UserDetail? userByEmail = null;
            UserDetail? userByPhone = null;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                userByEmail = await _context.UserDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.EmailAddress == request.Email && u.IsDeleted == false, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                userByPhone = await _context.UserDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber && u.IsDeleted == false, cancellationToken);
            }

            if (userByEmail != null && userByPhone != null && userByEmail.Id != userByPhone.Id)
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest,
                    AppMessages.Get("EmailAndPhoneMismatch", language));
            }

            if (userByEmail != null && userByPhone == null)
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest,
                    AppMessages.Get("EmailAlreadyExists", language));
            }

            if (userByPhone != null && userByEmail == null)
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest,
                    AppMessages.Get("PhoneAlreadyExists", language));
            }

            UserDetail? existingUser = userByEmail ?? userByPhone;
            string userId = string.Empty;

            if (existingUser == null)
            {
                var newUser = new UserDetail
                {
                    UserId = Guid.NewGuid().ToString(),
                    FullName = request.Name,
                    EmailAddress = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    IsActive = true,
                    Created = DateTime.UtcNow,
                    CreatedBy = createdBy.ToString(),
                    Role = nameof(Domain.Enums.Roles.User),
                };

                _context.UserDetails.Add(newUser);
                await _context.SaveChangesAsync(cancellationToken);

                userId = newUser.Id.ToString() ?? string.Empty;
            }
            else
            {
                userId = existingUser.Id.ToString() ?? string.Empty;
            }

            if (string.IsNullOrEmpty(userId))
                return Result<object>.Failure(StatusCodes.Status500InternalServerError, AppMessages.Get("UserIdRetrievalFailed", language));

            bool isAlreadyTeamMember = await _context.TeamMembers
                .AsNoTracking()
                .AnyAsync(tm => tm.UserId == userId && tm.IsDeleted == false, cancellationToken);

            if (isAlreadyTeamMember)
                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("AlreadyTeamMember", language));

            var teamMember = new TeamMember
            {
                UserId = userId,
                RoleType = request.RoleType,
                ContractId = contractIdCsv,
                Created = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = createdBy.ToString()
            };

            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("TeamSuccess", language), new
            {
                UserId = userId,
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.RoleType,
                request.ContractId,
                CreatedBy = createdBy
            });
        }
    }

}
//public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, Result<object>>
//   {
//       private readonly IApplicationDbContext _context;
//       private readonly IJwtService _jwtService;

//       public CreateTeamCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//       {
//           _context = context ?? throw new ArgumentNullException(nameof(context));
//           _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
//       }
//       public async Task<Result<object>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
//       {
//           if (string.IsNullOrWhiteSpace(request.Name))
//               return Result<object>.Failure(StatusCodes.Status400BadRequest, "Team name is required.");

//           var userIdFromJwt = _jwtService.GetUserId();
//           if (!int.TryParse(userIdFromJwt, out int createdBy) || createdBy == 0)
//               return Result<object>.Failure(StatusCodes.Status401Unauthorized, "Unauthorized request.");

//           if (string.IsNullOrWhiteSpace(request.RoleType))
//               return Result<object>.Failure(StatusCodes.Status400BadRequest, "RoleType is required.");

//           //if (request.ContractId.Count == 0)
//           //    return Result<object>.Failure(StatusCodes.Status400BadRequest, "ContractId is required.");
//           string? contractIdCsv = string.Empty;
//           if (request.ContractId != null)
//               contractIdCsv = string.Join(",", request.ContractId);


//           UserDetail? userByEmail = null;
//           UserDetail? userByPhone = null;

//           if (!string.IsNullOrWhiteSpace(request.Email))
//           {
//               userByEmail = await _context.UserDetails
//                   .AsNoTracking()
//                   .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);
//           }

//           if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
//           {
//               userByPhone = await _context.UserDetails
//                   .AsNoTracking()
//                   .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);
//           }

//           // Handle duplicates
//           if (userByEmail != null && userByPhone != null && userByEmail.Id != userByPhone.Id)
//           {
//               return Result<object>.Failure(StatusCodes.Status400BadRequest,
//                   "A user with this email and another with this phone number already exist. Please verify the details.");
//           }

//           if (userByEmail != null && userByPhone == null)
//           {
//               return Result<object>.Failure(StatusCodes.Status400BadRequest,
//                   "A user with this email already exists.");
//           }

//           if (userByPhone != null && userByEmail == null)
//           {
//               return Result<object>.Failure(StatusCodes.Status400BadRequest,
//                   "A user with this phone number already exists.");
//           }

//           // Use existing user if both match one
//           UserDetail? existingUser = userByEmail ?? userByPhone;
//           string userId = string.Empty;

//           if (existingUser == null)
//           {
//               var newUser = new UserDetail
//               {
//                   UserId = Guid.NewGuid().ToString(),
//                   FullName = request.Name,
//                   EmailAddress = request.Email,
//                   PhoneNumber = request.PhoneNumber,
//                   IsActive = true,
//                   Created = DateTime.UtcNow,
//                   CreatedBy = createdBy.ToString(),
//                   Role = nameof(Domain.Enums.Roles.User),
//               };

//               _context.UserDetails.Add(newUser);
//               await _context.SaveChangesAsync(cancellationToken);

//               userId = newUser.Id.ToString() ?? string.Empty;
//           }
//           else
//           {
//               userId = existingUser.Id.ToString() ?? string.Empty;
//           }

//           if (string.IsNullOrEmpty(userId))
//               return Result<object>.Failure(StatusCodes.Status500InternalServerError, "Failed to retrieve UserId.");

//           bool isAlreadyTeamMember = await _context.TeamMembers
//               .AsNoTracking()
//               .AnyAsync(tm => tm.UserId == userId, cancellationToken);

//           if (isAlreadyTeamMember)
//               return Result<object>.Failure(StatusCodes.Status400BadRequest, "This user is already a team member.");

//           var teamMember = new TeamMember
//           {
//               UserId = userId,
//               RoleType = request.RoleType,
//               ContractId = contractIdCsv,
//               Created = DateTime.UtcNow,
//               IsActive = true,
//               IsDeleted = false,
//               CreatedBy = createdBy.ToString()
//           };

//           _context.TeamMembers.Add(teamMember);
//           await _context.SaveChangesAsync(cancellationToken);

//           return Result<object>.Success(StatusCodes.Status200OK, "Team member created successfully.", new
//           {
//               UserId = userId,
//               request.Name,
//               request.Email,
//               request.PhoneNumber,
//               request.RoleType,
//               request.ContractId,
//               CreatedBy = createdBy
//           });
//       }


//   }
