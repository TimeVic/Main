using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Counters;

public class GetCountersResponse : IResponse
{
    public DashboardCountersDto Counters { get; set; } = new();
}
