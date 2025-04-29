using System.Runtime.InteropServices;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
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
        var fileUrls = await _fileService.UploadFilesForUser(files);
        if (fileUrls.Any())
        {
            return TypedResults.Ok(Result<List<string>>.Success(StatusCodes.Status200OK, "Success", fileUrls));
        }
        return TypedResults.BadRequest(Result<string>.Failure(StatusCodes.Status400BadRequest, "Something went wrong in saving the files."));
    }


    public class FileUploadRequest
    {
        public required List<IFormFile> Files { get; set; }
    }




    [HttpPost("UploadMultipleImages")]
    [IgnoreAntiforgeryToken]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> UploadMultipleImages([FromForm] FileUploadRequest files)
    {
        if (files == null || files.Files.Count == 0)
        {
            return BadRequest(new
            {
                Message = "No files were uploaded."
            });
        }

        var uploadedFiles = new List<string>();
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserUploads");

        try
        {
            // Ensure upload folder exists
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);

                // Try to set folder permission (Linux only)
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var chmod = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "/bin/bash",
                            Arguments = $"-c \"chmod -R 755 '{uploadPath}'\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    chmod.Start();
                    chmod.WaitForExit();
                }
            }

            foreach (var file in files.Files)
            {
                if (file.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}$${file.FileName}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var relativeUrl = $"/UserUploads/{fileName}";
                    uploadedFiles.Add(relativeUrl);
                }
            }

            return Ok(new
            {
                Message = "Files uploaded successfully.",
                Files = uploadedFiles
            });
        }
        catch (UnauthorizedAccessException uaEx)
        {
            return StatusCode(403, new
            {
                Error = "Access denied while saving the file.",
                Details = uaEx.Message
            });
        }
        catch (IOException ioEx)
        {
            return StatusCode(500, new
            {
                Error = "File system error occurred.",
                Details = ioEx.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "An unexpected error occurred.",
                Details = ex.Message
            });
        }
    }
}
