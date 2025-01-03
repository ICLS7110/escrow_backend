using AutoMapper;
using EscrowAPI.Books;

namespace EscrowAPI;

public class EscrowAPIApplicationAutoMapperProfile : Profile
{
    public EscrowAPIApplicationAutoMapperProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<CreateUpdateBookDto, Book>();
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
    }
}
