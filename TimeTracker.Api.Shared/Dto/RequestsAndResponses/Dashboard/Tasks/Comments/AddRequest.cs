using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments
{
    public class AddRequest : IRequest<TaskCommentDto>
    {
        [Required]
        [IsPositive]
        public long TaskId { get; set; }
        
        [Required]
        [StringLength(10000, MinimumLength = 1)]
        public string Comment { get; set; }
        
        public ICollection<long>? WatcherIds { get; set; }
    }
}
