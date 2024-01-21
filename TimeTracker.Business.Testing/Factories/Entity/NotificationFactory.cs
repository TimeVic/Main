using Bogus;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Notifications;
using TimeTracker.Business.Orm.Entities.Tasks;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

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
                .RuleFor(fake => fake.CreateTime, fake => fake.Date.Past().ToUniversalTime())
                .RuleFor(fake => fake.UpdateTime, fake => fake.Date.Past().ToUniversalTime());
        }

        public NotificationEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
