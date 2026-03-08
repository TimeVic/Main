using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks
{
    public class GetListFilterRequest : IRequest<GetListResponse>
    {
        public Guid? AssignedUserId { get; set; }
        
        [StringLength(100)]
        public string? SearchString { get; set; }

        public bool? IsArchived { get; set; } = false;
        
        public TaskStatus? Status { get; set; }

        public void Fill(GetListFilterRequest request)
        {
            AssignedUserId = request.AssignedUserId;
            SearchString = request.SearchString;
            IsArchived = request.IsArchived;
            Status = request.Status;
        }
    }
}
