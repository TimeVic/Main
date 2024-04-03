using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;

namespace TimeTracker.Api.FileStorage.Dto.RequestResponse.File;

public class GetRequest: IRequest<GetResponse>
{
    [Required]
    [MaxLength(255)]
    public required string Bucket { get; set; }
        
    [Required]
    [MaxLength(50)]
    public required string FileId { get; set; }
}
