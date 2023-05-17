using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks
{
    public class UpdateRequest : AddRequest
    {
        [Required]
        [IsPositive]
        public long TaskId { get; set; }
        
        [Required]
        [IsPositive]
        public long UserId { get; set; }

        public ICollection<long> TagIds { get; set; } = new List<long>();
        
        public void Fill(TaskDto dto)
        {
            TaskId = dto.Id;
            TaskListId = dto.TaskList.Id;
            Title = dto.Title;
            Description = dto.Description;
            StartTime = dto.StartTime;
            EndTime = dto.EndTime;
            Status = dto.Status;
            Priority = dto.Priority;
            IsArchived = dto.IsArchived;
            ExternalTaskId = dto.ExternalTaskId;
            UserId = dto.User.Id;
            TagIds = dto.Tags.Select(item => item.Id).ToList();
        }
    }
}
