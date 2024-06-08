using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.FileStorage.Dto.Entities;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Business.FileStorage.Mvc.Attribute;

namespace TimeTracker.Api.FileStorage.Dto.RequestResponse.Directory;

public class GetFilesRequest: IRequest<PaginatedListDto<FileStorageFileDto>>
{
    [Required]
    [IsStorageBucketName]
    public required string Bucket { get; set; }
        
    [StringLength(512)]
    public string? Directory { get; set; }
}
