using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities;

public class TimeEntryApprovalEntity : AEntity
{
    public virtual required TimeEntryEntity TimeEntry { get; set; }
    public virtual required UserEntity User { get; set; }
}
