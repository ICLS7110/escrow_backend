using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.Authentication;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.Authentication.Interfaces;
public interface IUserService
{
    Task<UserDetail> FindOrCreateUserAsync(string phoneNumber);
    Task<UserDetail> FindUserAsync(string phoneNumber);
}
