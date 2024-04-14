using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.FileStorage.Mvc.Attribute;

namespace TimeTracker.Api.FileStorage.Dto.RequestResponse.File;

public class GetRequest: IRequest<GetResponse>
{
    [Required]
    [IsStorageBucketName]
    public required string Bucket { get; set; }
        
    [Required]
    [MaxLength(50)]
    public required string FileId { get; set; }
}
