using Bogus;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class UserNotificationTokenEntityFactory : IDataFactory<UserNotificationTokenEntity>
    {
        private readonly Faker<UserNotificationTokenEntity> _factory;

        public UserNotificationTokenEntityFactory()
        {
            _factory = new Faker<UserNotificationTokenEntity>()
                .RuleFor(fake => fake.Token, fake => fake.Random.String2(100))
                .RuleFor(fake => fake.CreatedAt, fake => fake.Date.Past());
        }

        public UserNotificationTokenEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
