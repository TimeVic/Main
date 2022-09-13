using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project
{
    public class UpdateRequest : IRequest<ProjectDto>
    {
        [Required]
        [IsPositive]
        public long ProjectId { get; set; }

        [IsPositive]
        public long? ClientId { get; set; }
        
        [Required]
        [StringLength(256, MinimumLength = 2)]
        public string Name { get; set; }
        
        public bool IsBillableByDefault { get; set; }
        
        [IsPositive]
        public decimal? DefaultHourlyRate { get; set; }
    }
}
