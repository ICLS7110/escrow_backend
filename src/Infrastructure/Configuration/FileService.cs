using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.Internal.Util;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Escrow.Api.Application.Common.Interfaces;

using Escrow.Api.Infrastructure.OptionConfiguration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio.TwiML.Messaging;

namespace Escrow.Api.Infrastructure.Configuration;
public class FileService : IFileService
{
    private readonly IOptions<AWSS3> _awss3;
    public FileService(IOptions<AWSS3> awss3)
    {
        _awss3 = awss3;

    }
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
            if (file.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}$${file.FileName}";
                var filePath = Path.Combine(directoryPath, fileName);
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

    public async Task<List<string>> UploadFilesonAWS(List<IFormFile> files)
    {
        var fileUrls = new List<string>();

        var awsCredentials = new BasicAWSCredentials(_awss3.Value.AccessKey, _awss3.Value.SecretKey);
        using var s3Client = new AmazonS3Client(awsCredentials, Amazon.RegionEndpoint.GetBySystemName(_awss3.Value.Region));


        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_@@_{file.FileName}";

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = memoryStream,
                    BucketName = _awss3.Value.BucketName,
                    Key = fileName,
                    ContentType = file.ContentType,
                    CannedACL = S3CannedACL.PublicRead // Set access level
                };

                var transferUtility = new TransferUtility(s3Client);
                await transferUtility.UploadAsync(uploadRequest);

                var fileUrl = $"https://{_awss3.Value.BucketName}.s3.{_awss3.Value.Region}.amazonaws.com/{fileName}";
                fileUrls.Add(fileUrl);
            }
        }
        return fileUrls;
    }
}
