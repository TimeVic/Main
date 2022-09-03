﻿using TimeTracker.Business.Exceptions.Api;
using TimeTracker.Business.Exceptions.Common;
using ValidationException = TimeTracker.Business.Exceptions.Common.ValidationException;

namespace TimeTracker.Business.Exceptions;

public static class DomainException
{
    public static DataPermissionException DataPermissionException { get; } = new();
    
    public static ItemNotFoundException ItemNotFoundException { get; } = new();
    
    public static PermissionException PermissionException { get; } = new();
    
    public static TooManyRequestsException TooManyRequestsException { get; } = new();

    public static ValidationException ValidationException { get; } = new();
    
    public static RecordIsExistsException RecordIsExistsException { get; } = new();
    
    public static RecordNotFoundException RecordNotFoundException { get; } = new();
    
    public static ServerException ServerException { get; } = new();
    
    public static TooManyRecordsException TooManyRecordsException { get; } = new();
    
    public static UserNotAuthorizedException UserNotAuthorizedException { get; } = new();
}
