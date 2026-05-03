using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Business.Orm.Entities.Workspaces
{
    public class WorkspaceEntity: AEntity
    {   
        public virtual required string Name { get; set; }
        public virtual bool IsDefault { get; set; }

        public virtual required string TimeZone { get; set; }
        public virtual string? Description { get; set; }
        
        #region Relationships

        public virtual required CurrencyEntity Currency { get; set; }
        public virtual required UserEntity CreatedUser { get; set; }
        public virtual ICollection<ClientEntity> Clients { get; set; } = new List<ClientEntity>();
        public virtual ICollection<ProjectEntity> Projects { get; set; } = new List<ProjectEntity>();
        public virtual ICollection<TimeEntryEntity> TimeEntries { get; set; } = new List<TimeEntryEntity>();
        public virtual ICollection<WorkspaceSettingsClickUpEntity> SettingsClickUp { get; set; } = new List<WorkspaceSettingsClickUpEntity>();
        public virtual ICollection<WorkspaceSettingsRedmineEntity> SettingsRedmine { get; set; } = new List<WorkspaceSettingsRedmineEntity>();
        public virtual ICollection<WorkspaceSettingsJiraEntity> SettingsJira { get; set; } = new List<WorkspaceSettingsJiraEntity>();
        public virtual ICollection<WorkspaceMemberEntity> Members { get; set; } = new List<WorkspaceMemberEntity>();
        public virtual ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();

        #endregion
        
        #region Integration - ClickUp
        
        public virtual WorkspaceSettingsClickUpEntity? GetClickUpSettings(Guid userId)
        {
            return SettingsClickUp.FirstOrDefault(
                item => item.User.Id == userId
            );
        }

        public virtual WorkspaceSettingsClickUpEntity? GetClickUpSettings(UserEntity user)
        {
            return GetClickUpSettings(user.Id);
        }
        
        public virtual WorkspaceSettingsJiraEntity? GetJiraSettings(Guid userId)
        {
            return SettingsJira.FirstOrDefault(
                item => item.User.Id == userId
            );
        }
        
        public virtual WorkspaceSettingsJiraEntity? GetJiraSettings(UserEntity user)
        {
            return GetJiraSettings(user.Id);
        }
        
        public virtual bool IsIntegrationClickUpActive(Guid userId)
        {
            return GetClickUpSettings(userId)?.IsActive ?? false;
        }
        
        public virtual bool IsIntegrationJiraActive(Guid userId)
        {
            return GetJiraSettings(userId)?.IsActive ?? false;
        }
        
        #endregion
        
        #region Integration - Redmine
        
        public virtual WorkspaceSettingsRedmineEntity? GetRedmineSettings(Guid userId)
        {
            return SettingsRedmine.FirstOrDefault(
                item => item.User.Id == userId
            );
        }

        public virtual WorkspaceSettingsRedmineEntity? GetRedmineSettings(UserEntity user)
        {
            return GetRedmineSettings(user.Id);
        }
        
        public virtual bool IsIntegrationRedmineActive(Guid userId)
        {
            return GetRedmineSettings(userId)?.IsActive ?? false;
        }
        
        #endregion
        
        #region Other

        public virtual bool ContainsProject(ProjectEntity? project)
        {
            if (project == null)
            {
                return false;
            }
            return Projects.Any(item => item.Id == project.Id);
        }
        
        #endregion
    }
}
