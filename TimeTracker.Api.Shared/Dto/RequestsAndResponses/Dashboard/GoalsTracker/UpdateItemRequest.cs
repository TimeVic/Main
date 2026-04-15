using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker
{
    public class UpdateItemRequest : IRequest<GoalsTrackerItemDto>
    {
        [RequiredNonEmpty]
        public Guid GoalsTrackerItemId { get; set; }
        
        [IsPositive]
        public int NumberOfTimes { get; set; }

        [RequiredNonEmpty]
        [StringLength(256, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }
}
