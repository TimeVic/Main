using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class ClientShareReportSettingsResponse : IResponse
{
    public bool IsActive { get; set; }

    public bool IsShowTasks { get; set; }

    public string Token { get; set; } = string.Empty;

    public string ShareUrl { get; set; } = string.Empty;
}
