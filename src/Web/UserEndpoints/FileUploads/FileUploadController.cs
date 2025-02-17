using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.ResultHandler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Web.UserEndpoints.FileUploads;
[Route("api/files")]
[ApiController]
public class FileUploadController : ControllerBase
{
    private readonly IFileService _fileService;
    public FileUploadController(IFileService fileService)
    {
            _fileService = fileService;
    }

    [HttpPost]
    [Authorize]
    [IgnoreAntiforgeryToken]
    [Route("Upload-Files")]
    public async Task<IResult> UploadFiles([FromForm] List<IFormFile> files)
    {

        if (files == null || files.Count == 0)
        {
            return TypedResults.BadRequest(Result<string>.Failure(StatusCodes.Status400BadRequest, "No files Found."));

        }
        var fileUrls = await _fileService.UploadFilesonAWS(files);
        if (fileUrls.Any())
        {
            return TypedResults.Ok(Result<List<string>>.Success(StatusCodes.Status200OK,"Success", fileUrls));
        }
        return TypedResults.BadRequest(Result<string>.Failure(StatusCodes.Status400BadRequest, "Something went wrong in saving the files."));
    }
}
