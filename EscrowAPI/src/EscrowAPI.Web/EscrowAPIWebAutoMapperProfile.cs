using AutoMapper;
using EscrowAPI.Books;

namespace EscrowAPI.Web;

public class EscrowAPIWebAutoMapperProfile : Profile
{
    public EscrowAPIWebAutoMapperProfile()
    {
        CreateMap<BookDto, CreateUpdateBookDto>();
        
        //Define your object mappings here, for the Web project
    }
}
