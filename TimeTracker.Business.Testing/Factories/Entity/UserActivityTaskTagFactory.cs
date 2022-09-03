using Bogus;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class UserActivityTaskTagFactory : IDataFactory<UserEntity>
    {
        private readonly Faker<UserEntity> _factory;

        public UserActivityTaskTagFactory()
        {
            _factory = new Faker<UserEntity>()
                .RuleFor(fake => fake.UserName, fake => fake.Random.String2(100))
                .RuleFor(fake => fake.Email, fake => fake.Person.Email)
                .RuleFor(fake => fake.CreateTime, fake => fake.Date.Past())
                .RuleFor(fake => fake.UpdateTime, fake => fake.Date.Past());
        }

        public UserEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
