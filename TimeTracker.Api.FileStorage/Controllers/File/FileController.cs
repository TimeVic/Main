using AspNetCore.ApiControllers.Extensions;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.FileStorage.Dto.RequestResponse.File;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.FileStorage.Commons.Constants;
using TimeTracker.Business.FileStorage.Mvc.Controllers;
using TimeTracker.Business.FileStorage.Services.Storage;
using TimeTracker.Business.Orm.Dao.FileStorage;

namespace TimeTracker.Api.FileStorage.Controllers.File;

[ApiController]
[Route("/[controller]")]
public class FileController: BaseStorageController
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileStorageFileDao _storageFileDao;

    public FileController(
        ILifetimeScope scope,
        IFileStorageService fileStorageService,
        IFileStorageFileDao storageFileDao    
    ): base(scope)
    {
        _fileStorageService = fileStorageService;
        _storageFileDao = storageFileDao;
    }

    [HttpGet("{bucket:maxlength(255)}/{fileId:maxlength(50)}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromRoute] GetRequest request)
    {
        // var user = await _storageSecurityService.GetCurrentUser();
        var file = await _storageFileDao.GetByExternalId(request.Bucket, request.FileId);
        if (file == null)
        {
            throw new DataValidationException("Bucket was not found or not available for this user");
        }
        
        // Add file info
        HttpContext.Response.Headers.Append(FileInfoHttpHeader.Directory, file.Directory?.FullPath);
        HttpContext.Response.Headers.Append(FileInfoHttpHeader.FileName, file.OriginalFileName);
        
        var fileStream = await _fileStorageService.DownloadToStream(file);
        fileStream.PrepareToCopy();
        return new FileStreamResult(fileStream, file.MimeType);
    }
}
