using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks
{
    public class AddRequest : IRequest<TaskDto>
    {
        [Required]
        [IsPositive]
        public long TaskListId { get; set; }
        
        [Required]
        [StringLength(1024, MinimumLength = 1)]
        public string Title { get; set; }
        
        [StringLength(10000)]
        public string? Description { get; set; }
        
        [StringLength(512)]
        public string? ExternalTaskId { get; set; }
        
        [IsFutureOrNowDate]
        public DateTime? NotificationTime { get; set; }
    
        public bool IsDone { get; set; }
    
        public bool IsArchived { get; set; }
    }
}
