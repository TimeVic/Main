using TimeTracker.Business.Common.Dto;

namespace TimeTracker.Client.Core.Store.Dashboard;

public record struct FetchCountersAction();

public record struct SetCountersAction(DashboardCountersDto Counters);
