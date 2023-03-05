using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using Microsoft.AspNetCore.Http;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;

public class GetListRequest: IRequest<GetListResponse>
{
    [Required]
    public long EntityId { get; set; }
        
    [Required]
    public StorageEntityType EntityType { get; set; }
}
