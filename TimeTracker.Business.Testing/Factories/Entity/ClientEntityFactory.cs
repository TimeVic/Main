using Bogus;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class ClientEntityFactory : IDataFactory<ClientEntity>
    {
        private readonly Faker<ClientEntity> _factory;

        public ClientEntityFactory()
        {
            _factory = new Faker<ClientEntity>()
                .RuleFor(fake => fake.Name, fake => fake.Random.String2(100))
                .RuleFor(fake => fake.CreateTime, fake => fake.Date.Past())
                .RuleFor(fake => fake.UpdateTime, fake => fake.Date.Past());
        }

        public ClientEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
