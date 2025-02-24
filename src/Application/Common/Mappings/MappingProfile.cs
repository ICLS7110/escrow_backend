
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities;
namespace Escrow.Api.Application.Common.Mappings;   
public class MappingProfile : Profile       
{
    public MappingProfile()
    {
    
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<Bank, BankDto>().ReverseMap();


    }
}
