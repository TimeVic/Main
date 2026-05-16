using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Business.Common.Constants.Storage;

namespace TimeTracker.Api.Dto.RequestsAndResponses.Storage;

public class GetFileRequest: IRequest<FileResponse>
{
    [Required]
    public Guid FileId { get; set; }

    public StorageImageSize? ImageSize { get; set; }
}
