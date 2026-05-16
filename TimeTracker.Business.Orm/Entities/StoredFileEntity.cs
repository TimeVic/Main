using Domain.Abstractions;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Business.Orm.Entities
{
    public class StoredFileEntity: AEntity
    {   
        public virtual StoredFileType Type { get; set; }
        public virtual required string CloudFilePath { get; set; }
        public virtual string? ThumbCloudFilePath { get; set; }
        public virtual string? Extension { get; set; }
        public virtual required string MimeType { get; set; }
        public virtual required string OriginalFileName { get; set; }
        public virtual string? Title { get; set; }
        public virtual string? Description { get; set; }
        public virtual long? Size { get; set; }
        public virtual ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
        public virtual ICollection<TaskCommentEntity> TaskComments { get; set; } = new List<TaskCommentEntity>();
        
        #region Calculated

        public virtual IEntity? Relationship
        {
            get
            {
                if (Tasks.Any())
                {
                    return Tasks.First();
                }
                if (TaskComments.Any())
                {
                    return TaskComments.First();
                }

                return null;
            }
        }
        
        public virtual string Url => $"/dashboard/storage/file/{Id}";
        
        public virtual string ThumbUrl => $"/dashboard/storage/file/thumbnail/{Id}";

        #endregion
    }
}
