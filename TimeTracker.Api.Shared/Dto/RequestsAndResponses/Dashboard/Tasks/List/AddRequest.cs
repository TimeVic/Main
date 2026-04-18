using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List
{
    public class AddRequest : IRequest<TaskListDto>
    {
        [Required]
        public Guid ProjectId { get; set; }
        
        [Required]
        [StringLength(1024, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }
}
