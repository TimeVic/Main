using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "task_lists")]
    public class TaskListEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "name", Length = 1024, NotNull = true)]
        public virtual string Name { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "is_archived", NotNull = true)]
        public virtual bool IsArchived { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(ProjectEntity), 
            Column = "project_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual ProjectEntity Project { get; set; }
        
        public virtual void SetProject(ProjectEntity project)
        {
            if (Project?.Id == project?.Id)
            {
                return;
            }

            Project = project;
            Project.TaskLists.Add(this);
        }
    }
}
