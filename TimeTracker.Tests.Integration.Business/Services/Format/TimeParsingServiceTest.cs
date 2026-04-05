using Autofac;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Format;

public class TimeParsingServiceTest: BaseTest
{
    private readonly ITimeParsingService _timeParsingService;

    public TimeParsingServiceTest(): base()
    {
        _timeParsingService = Scope.Resolve<ITimeParsingService>();
    }
}
