using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IUserDao
{
    Task<UserEntity?> GetExistsByUserName(string userName);
    Task<UserEntity?> GetExistsByEmail(string email);
    Task<UserEntity> CreatePendingUser(string email);
}
