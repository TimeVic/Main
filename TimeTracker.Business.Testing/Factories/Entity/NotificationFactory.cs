using Bogus;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities.Notifications;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class NotificationFactory : IDataFactory<NotificationEntity>
    {
        private readonly Faker<NotificationEntity> _factory;

        public NotificationFactory()
        {
            _factory = new Faker<NotificationEntity>()
                .RuleFor(fake => fake.Type, fake => NotificationActionType.Reminder)
                .RuleFor(fake => fake.IsRead, fake => false)
                .RuleFor(fake => fake.CreatedAt, fake => fake.Date.Past().ToUniversalTime())
                .RuleFor(fake => fake.UpdatedAt, fake => fake.Date.Past().ToUniversalTime());
        }

        public NotificationEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
