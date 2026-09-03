using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.Tasks;

public class TaskSubTaskEntity : AEntity
{
    public virtual required TaskEntity Task { get; set; }

    public virtual required string Title { get; set; }

    public virtual bool IsCompleted { get; set; }

    public virtual int PositionIndex { get; set; }
}
