using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.GoalsTracker
{
    public class GoalsTrackerNoteEntity: AEntity
    {
        public virtual string Text { get; set; } = string.Empty;
        public virtual bool IsArchived { get; set; } = false;

        #region Relationships

        public virtual required GoalsTrackerEntity Tracker { get; set; }

        #endregion
    }
}
