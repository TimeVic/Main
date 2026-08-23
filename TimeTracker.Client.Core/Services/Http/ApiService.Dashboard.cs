using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Counters;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Init;

namespace TimeTracker.Client.Core.Services.Http;

public partial class ApiService
{
    public async Task<DashboardInitResponse?> DashboardInitAsync()
    {
        return await PostAsync<DashboardInitResponse>(
            ApiUrl.DashboardInit,
            new DashboardInitRequest()
        );
    }

    public async Task<GetCountersResponse?> DashboardGetCountersAsync()
    {
        return await PostAsync<GetCountersResponse>(
            ApiUrl.DashboardCounters,
            new GetCountersRequest()
        );
    }
}
