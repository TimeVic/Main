using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Task
{
    public class AddTaskListRequest : IRequest<TaskListDto>
    {
        [Required]
        [IsPositive]
        public long? ProjectId { get; set; }
        
        [StringLength(1024, MinimumLength = 1)]
        public string Name { get; set; }
    }
}
