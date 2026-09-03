using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.SubTasks
{
    public class AddRequest : IRequest<TaskSubTaskDto>
    {
        [Required]
        public Guid TaskId { get; set; }

        [Required]
        [StringLength(512, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;
    }
}
