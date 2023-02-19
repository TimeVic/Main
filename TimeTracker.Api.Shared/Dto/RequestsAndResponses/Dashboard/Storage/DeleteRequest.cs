using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using Microsoft.AspNetCore.Http;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Storage;

public class DeleteRequest: IRequest
{
    [Required]
    [IsPositive]
    public long Id { get; set; }
}
