using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks
{
    public class AddRequest : IRequest<TaskDto>
    {
        [IsPositive]
        public Guid? TimeEntryId { get; set; } 
        
        [Required]
        [IsPositive(ErrorMessage = "Task list is required")]
        public Guid TaskListId { get; set; }
        
        [StringLength(1024, MinimumLength = 1)]
        public string? Title { get; set; }
        
        [StringLength(512)]
        public string? ExternalTaskId { get; set; }
        
        [StringLength(10000)]
        public string? Description { get; set; }
        
        public DateTime? StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.ToDo;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    
        public bool IsArchived { get; set; }
    }
}
