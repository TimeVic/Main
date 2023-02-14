using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace TimeTracker.Api.Dto.RequestsAndResponses.Storage;

public class GetFileRequest: IRequest<FileResponse>
{
    [Required]
    public long FileId { get; set; }
}
