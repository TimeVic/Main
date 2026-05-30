using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.User;

public class UserSocialLoginEntity : AEntity
{
    public virtual string? GoogleId { get; set; }
    public virtual string? GoogleAccessToken { get; set; }
    public virtual string? GoogleRefreshToken { get; set; }
    public virtual DateTime? GoogleConnectedAt { get; set; }

    public virtual string? FacebookId { get; set; }
    public virtual string? FacebookAccessToken { get; set; }
    public virtual string? FacebookRefreshToken { get; set; }
    public virtual DateTime? FacebookConnectedAt { get; set; }
    
    public virtual string? AppleId { get; set; }
    public virtual string? AppleAccessToken { get; set; }
    public virtual string? AppleRefreshToken { get; set; }
    public virtual DateTime? AppleConnectedAt { get; set; }
    
    #region Relationships
    public virtual required UserEntity User { get; set; }
    #endregion
}
