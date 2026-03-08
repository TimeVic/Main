using Bogus;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class UserEntityFactory : IDataFactory<UserEntity>
    {
        private readonly Faker<UserEntity> _factory;

        public UserEntityFactory()
        {
            _factory = new Faker<UserEntity>()
                .RuleFor(fake => fake.UserName, fake => fake.Random.String2(100))
                .RuleFor(fake => fake.Email, fake => fake.Person.Email)
                .RuleFor(fake => fake.Timezone, fake => "UTC")
                .RuleFor(fake => fake.CreatedAt, fake => fake.Date.Past())
                .RuleFor(fake => fake.UpdatedAt, fake => fake.Date.Past());
        }

        public UserEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
