﻿using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface IUserSeeder: IDomainService
{
    Task<UserEntity> CreateActivatedAsync(string password = "test password");
}
