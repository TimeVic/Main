using Domain.Abstractions;

namespace TimeTracker.Business.Testing.Factories;

public interface IDataFactory<TType>: IDomainService where TType : class
{
    TType Generate();
}
