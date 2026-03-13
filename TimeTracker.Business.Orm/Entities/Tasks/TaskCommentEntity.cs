using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Entities.Tasks
{
    public class TaskCommentEntity: AEntity
    {
        public virtual required string Comment { get; set; }
        public virtual bool IsArchived { get; set; }

        #region Relationships

        public virtual UserEntity? User { get; set; }
        public virtual required TaskEntity Task { get; set; }
        public virtual ICollection<UserEntity> Watchers { get; set; } = new List<UserEntity>();
        public virtual ICollection<StoredFileEntity> Attachments { get; set; } = new List<StoredFileEntity>();

        #endregion
    }
}
