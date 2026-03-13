using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities.GoalsTracker
{
    public class GoalsTrackerEntity: AEntity
    {   
        public virtual int Year { get; set; }
        public virtual int Month { get; set; }

        #region Relationships

        public virtual required WorkspaceEntity Workspace { get; set; }
        public virtual required UserEntity User { get; set; }
        public virtual ICollection<GoalsTrackerItemEntity> Items { get; set; } = new List<GoalsTrackerItemEntity>();
        public virtual ICollection<GoalsTrackerNoteEntity> Notes { get; set; } = new List<GoalsTrackerNoteEntity>();

        #endregion
        
        #region Calculated

        public virtual IEnumerable<GoalsTrackerItemEntity> ActiveItems => Items.Where(item => !item.IsArchived);

        public virtual int DaysInCurrentMonth => DateTime.DaysInMonth(Year, Month);
        
        #endregion
    }
}
