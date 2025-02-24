namespace Escrow.Api.Application.Interfaces;

using Escrow.Api.Domain.Entities;


public interface IUserService
{
    Task<User> CreateUserAsync(string phoneNumber);
    Task<User> FindOrCreateUserAsync(string phoneNumber);
    Task<User?> FindUserAsync(string phoneNumber);
}
