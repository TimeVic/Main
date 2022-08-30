using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Db;

public class ConnectionTest: BaseTest
{
    [Fact]
    public void TestDbConnection()
    {
        DbSessionProvider.PerformCommitAsync();
    }
}
