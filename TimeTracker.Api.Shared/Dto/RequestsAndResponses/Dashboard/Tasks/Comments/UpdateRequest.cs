using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments
{
    public class UpdateRequest : IRequest<TaskCommentDto>
    {
        [Required]
        public Guid CommentId { get; set; }
        
        [Required]
        [StringLength(10000, MinimumLength = 1)]
        public string Comment { get; set; }
        
        public IEnumerable<Guid>? WatcherIds { get; set; }

        public UpdateRequest()
        {
        }
        
        public UpdateRequest(Guid commentId, AddRequest addRequest)
        {
            CommentId = commentId;
            Comment = addRequest.Comment;
            WatcherIds = addRequest.WatcherIds;
        }
    }
}
