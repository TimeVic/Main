using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users
{
    public class SetNotificationTokenRequest : IRequest
    {
        [Required]
        [StringLength(900, MinimumLength = 10)]
        public string Token { get; set; } = string.Empty;
    }
}
