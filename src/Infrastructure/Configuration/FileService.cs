using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.ResultHandler;
using Microsoft.AspNetCore.Http;
using Twilio.TwiML.Messaging;

namespace Escrow.Api.Infrastructure.Configuration;
public class FileService : IFileService
{
    public async Task<List<string>> UploadFilesForUser(List<IFormFile> files)
    {
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserUploads");
        if (!Directory.Exists(directoryPath))
        { 
            Directory.CreateDirectory(directoryPath);
        }

        var fileUrls = new List<string>();
        foreach (var file in files)
        { 
            if(file.Length> 0)
            {
                var fileName = $"{Guid.NewGuid()}$${file.FileName}";
                var filePath= Path.Combine(directoryPath, fileName);
                using (var strem = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(strem);
                }

                var fileUrl = $"UserUploads/{fileName}";
                fileUrls.Add(fileUrl);
            }
        }
        return fileUrls;
    }
}
