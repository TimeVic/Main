using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.Tasks
{
    public class TaskListEntity: AEntity
    {
        public virtual required string Name { get; set; }
        public virtual bool IsArchived { get; set; }
        
        [ManyToOne(
            ClassType = typeof(ProjectEntity), 
            Column = "project_id", 
            Lazy = Laziness.Proxy,
            Cascade = "none"
        )]
        public virtual required ProjectEntity Project { get; set; }
        
        public virtual void SetProject(ProjectEntity project)
        {
            if (Project.Id == project.Id)
            {
                return;
            }

            Project = project;
            Project.TaskLists.Add(this);
        }
    }
}
