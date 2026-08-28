using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry
{
    public class StartRequest : IRequest<StartResponse>
    {
        public Guid? ProjectId { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }

        public Guid? InternalTaskId { get; set; }
        
        public bool? IsBillable { get; set; }
        
        [IsPositive]
        public decimal? HourlyRate { get; set; }
        
    }
}
