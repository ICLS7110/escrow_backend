using System;
using EscrowAPI.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EscrowAPI.Books;

public class BookAppService :
    CrudAppService<
        Book, //The Book entity
        BookDto, //Used to show books
        Guid, //Primary key of the book entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateBookDto>, //Used to create/update a book
    IBookAppService //implement the IBookAppService
{
    public BookAppService(IRepository<Book, Guid> repository)
        : base(repository)
    {
        GetPolicyName = EscrowAPIPermissions.Books.Default;
        GetListPolicyName = EscrowAPIPermissions.Books.Default;
        CreatePolicyName = EscrowAPIPermissions.Books.Create;
        UpdatePolicyName = EscrowAPIPermissions.Books.Edit;
        DeletePolicyName = EscrowAPIPermissions.Books.Delete;
    }
}