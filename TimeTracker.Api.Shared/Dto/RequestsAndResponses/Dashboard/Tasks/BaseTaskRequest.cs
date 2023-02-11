using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;

public class BaseTaskRequest : IRequest<TaskDto>
{
    [Required]
    [StringLength(1024, MinimumLength = 1)]
    public string Title { get; set; }
        
    [StringLength(10000)]
    public string? Description { get; set; }
        
    [IsFutureOrNowDate]
    public DateTime? NotificationTime { get; set; }
}
