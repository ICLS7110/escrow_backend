

using Microsoft.AspNetCore.Http;


namespace Escrow.Api.Application.Common.Interfaces;
public interface IFileService
{
    Task<List<string>>  UploadFilesForUser(List<IFormFile> files);
    Task<List<string>> UploadFilesonAWS(List<IFormFile> files);
}
