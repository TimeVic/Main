using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker
{
    public class SetCompletionRequest : IRequest<GoalsTrackerCompletionMarkerDto>
    {
        [Required]
        [IsPositive]
        public long GoalsTrackerItemId { get; set; }
        
        [Required]
        [IsPositive]
        public int DayOfMonth { get; set; }
        
        public bool IsChecked { get; set; }
    }
}
