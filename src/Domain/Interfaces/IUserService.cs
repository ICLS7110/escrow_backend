using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.Authentication;

namespace Escrow.Api.Domain.Interfaces;
public interface IUserService
{
    Task<User> FindOrCreateUserAsync(string phoneNumber);
    Task<User> FindUserAsync(string phoneNumber);
}
