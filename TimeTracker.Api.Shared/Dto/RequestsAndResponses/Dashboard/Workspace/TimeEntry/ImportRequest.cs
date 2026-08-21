using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using Microsoft.AspNetCore.Http;
using TimeTracker.Business.Common.Constants.Import;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace.TimeEntry;

public class ImportRequest : IRequest<ImportResponse>
{
    [Required]
    public TimeEntryImportSourceType SourceType { get; set; } = TimeEntryImportSourceType.Clockify;

    public bool IsBillable { get; set; }

    public decimal? HourlyRate { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!;
}
