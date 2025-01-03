using Microsoft.AspNetCore.Builder;
using EscrowAPI;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("EscrowAPI.Web.csproj"); 
await builder.RunAbpModuleAsync<EscrowAPIWebTestModule>(applicationName: "EscrowAPI.Web");

public partial class Program
{
}
