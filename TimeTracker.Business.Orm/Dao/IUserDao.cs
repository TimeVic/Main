using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IUserDao: IDomainService
{
    Task<UserEntity?> GetExistsByUserName(string userName);
    Task<UserEntity?> GetExistsByEmail(string email);
    Task<UserEntity> CreatePendingUser(string email);
}
