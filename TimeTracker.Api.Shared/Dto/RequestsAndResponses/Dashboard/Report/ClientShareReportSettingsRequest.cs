using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;

public class ClientShareReportSettingsRequest : IRequest<ClientShareReportSettingsResponse>
{
    public Guid ClientId { get; set; }

    public bool IsActive { get; set; }

    public bool IsShowTasks { get; set; } = true;

    public bool IsUpdateSettings { get; set; }

    public bool IsRegenerateToken { get; set; }
}
