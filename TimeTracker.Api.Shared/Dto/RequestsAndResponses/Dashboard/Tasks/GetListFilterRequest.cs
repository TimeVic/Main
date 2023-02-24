using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks
{
    public class GetListFilterRequest : IRequest<GetListResponse>
    {
        [IsPositive]
        public long? AssignedUserId { get; set; }
        
        [StringLength(100)]
        public string? SearchString { get; set; }
        
        public bool? IsArchived { get; set; }
        
        public bool? IsDone { get; set; }

        public void Fill(GetListFilterRequest request)
        {
            AssignedUserId = request.AssignedUserId;
            SearchString = request.SearchString;
            IsArchived = request.IsArchived;
            IsDone = request.IsDone;
        }
    }
}
