using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Infrastructure.Helpers;
public interface IUserService
{
    Task<bool> UserExistsAsync(string email);
    Task CreateUserAsync(CreateUserDto dto);
}
