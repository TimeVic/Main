using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Services.Auth;

public class PasswordService: IPasswordService
{
    public UserEntity SetUserPassword(UserEntity user, string password)
    {
        user.PasswordSalt = SecurityUtil.GenerateSalt(32);
        user.PasswordHash = SecurityUtil.GeneratePasswordHash(password, user.PasswordSalt);
        return user;
    }
    
    public bool ValidatePassword(UserEntity user, string password)
    {
        if (user.PasswordSalt == null || user.PasswordHash == null)
        {
            return false;
        }
        var passwordHash = SecurityUtil.GeneratePasswordHash(password, user.PasswordSalt);
        return user.PasswordHash.CompareTo(passwordHash);
    }
}
