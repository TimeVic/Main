using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks
{
    public class GetMyListRequest : IRequest<GetListResponse>
    {
        public Guid WorkspaceId { get; set; }
        
        [StringLength(100)]
        public string? SearchString { get; set; }

        public DateTime? StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public ICollection<TaskStatus>? Statuses { get; set; } = new List<TaskStatus>();
    }
}
