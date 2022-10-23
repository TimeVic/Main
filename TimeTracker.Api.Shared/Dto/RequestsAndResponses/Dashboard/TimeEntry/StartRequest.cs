using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry
{
    public class StartRequest : IRequest<TimeEntryDto>
    {
        [Required]
        [IsPositive]
        public long WorkspaceId { get; set; }
        
        [IsPositive]
        public long? ProjectId { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; } 
        
        public bool IsBillable { get; set; }
        
        [IsFutureOrNowDate]
        public DateTime Date { get; set; }
        
        [IsCorrectTimeEntryTime]
        public TimeSpan StartTime { get; set; }
    }
}
