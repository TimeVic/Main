using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities;

public class TimeEntryRejectEntity : AEntity
{
    public virtual required TimeEntryEntity TimeEntry { get; set; }
    public virtual required UserEntity User { get; set; }
    public virtual required string Reason { get; set; }
}
