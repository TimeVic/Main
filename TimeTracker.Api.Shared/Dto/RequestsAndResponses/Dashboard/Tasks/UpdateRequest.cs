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

        public void Fill(TaskDto dto)
        {
            TaskId = dto.Id;
            TaskListId = dto.TaskList.Id;
            Title = dto.Title;
            Description = dto.Description;
            NotificationTime = dto.NotificationTime;
            IsDone = dto.IsDone;
            IsArchived = dto.IsArchived;
            UserId = dto.User.Id;
        }
    }
}
