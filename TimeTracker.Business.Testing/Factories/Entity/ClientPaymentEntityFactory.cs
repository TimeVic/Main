using Bogus;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Factories.Entity;

internal class ClientPaymentEntityFactory : IDataFactory<ClientPaymentEntity>
{
    private readonly Faker<ClientPaymentEntity> _factory;

    public ClientPaymentEntityFactory()
    {
        _factory = new Faker<ClientPaymentEntity>()
            .RuleFor(fake => fake.PaymentTime, fake => fake.Date.Past().ToUniversalTime())
            .RuleFor(fake => fake.Amount, fake => fake.Random.Decimal(1, 200))
            .RuleFor(fake => fake.Description, fake => fake.Lorem.Sentence(3))
            .RuleFor(fake => fake.CreatedAt, fake => fake.Date.Past().ToUniversalTime())
            .RuleFor(fake => fake.UpdatedAt, fake => fake.Date.Past().ToUniversalTime());
    }

    public ClientPaymentEntity Generate()
    {
        return _factory.Generate();
    }
}
