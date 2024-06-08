using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments
{
    public class UpdateRequest : IRequest<TaskCommentDto>
    {
        [Required]
        [IsPositive]
        public long CommentId { get; set; }
        
        [Required]
        [StringLength(10000, MinimumLength = 1)]
        public string Comment { get; set; }
        
        public IEnumerable<long>? WatcherIds { get; set; }

        public UpdateRequest()
        {
        }
        
        public UpdateRequest(long commentId, AddRequest addRequest)
        {
            CommentId = commentId;
            Comment = addRequest.Comment;
            WatcherIds = addRequest.WatcherIds;
        }
    }
}
