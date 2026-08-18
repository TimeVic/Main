using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities;

public class SharedClientReportEntity : AEntity
{
    public virtual required string Token { get; set; }

    public virtual bool IsActive { get; set; }

    public virtual bool IsShowTasks { get; set; } = true;

    public virtual required ClientEntity Client { get; set; }
}
