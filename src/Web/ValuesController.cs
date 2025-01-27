using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.UserPanel.Queries.GetUsers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Web;
[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    public async Task<Ok<PaginatedList<UserDetailDto>>> GetUserDetails(ISender sender, [AsParameters] GetUserDetailsQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

}
