using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.Authentication;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.Authentication.Interfaces;
public interface IUserService
{
    Task<Result<UserDetail>> CreateUserAsync(string phoneNumber);
    Task<Result<UserDetail>> FindOrCreateUserAsync(string phoneNumber);
    Task<Result<UserDetail>> FindUserAsync(string phoneNumber);
}
