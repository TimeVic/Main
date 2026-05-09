using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry
{
    public class SetRequest : IRequest<TimeEntryDto>
    {
        public Guid? Id { get; set; }
        public Guid? ProjectId { get; set; }

        public string? Description { get; set; }
    
        public decimal? HourlyRate { get; set; }
    
        public bool IsBillable { get; set; }
    
        public DateTime StartTime { get; set; }
    
        public DateTime? EndTime { get; set; }
    }
}
