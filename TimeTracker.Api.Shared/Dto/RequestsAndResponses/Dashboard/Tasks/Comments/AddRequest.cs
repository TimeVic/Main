using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments
{
    public class AddRequest : IRequest<TaskCommentDto>
    {   
        [Required]
        public Guid TaskId { get; set; }

        [Required]
        [StringLength(10000, MinimumLength = 1)]
        public string Comment { get; set; } = "";

        public IEnumerable<Guid> WatcherIds { get; set; } = new List<Guid>();

        public void Fill(TaskCommentDto comment)
        {
            TaskId = comment.Task.Id;
            Comment = $"{comment.Comment}";
            WatcherIds = comment.Watchers?.Select(item => item.Id).ToList() ?? new List<Guid>();
        }
    }
}
