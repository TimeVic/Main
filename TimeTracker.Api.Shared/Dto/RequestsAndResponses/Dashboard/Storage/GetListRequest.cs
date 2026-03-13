using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants.Storage;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;

public class GetListRequest: IRequest<GetListResponse>
{
    [Required]
    public Guid EntityId { get; set; }
        
    [Required]
    public StorageEntityType EntityType { get; set; }
}
