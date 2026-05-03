using Bogus;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class MemberPaymentEntityFactory : IDataFactory<MemberPaymentEntity>
    {
        private readonly Faker<MemberPaymentEntity> _factory;

        public MemberPaymentEntityFactory()
        {
            _factory = new Faker<MemberPaymentEntity>()
                .RuleFor(fake => fake.PaymentTime, fake => fake.Date.Past().ToUniversalTime())
                .RuleFor(fake => fake.Amount, fake => fake.Random.Decimal(1, 200))
                .RuleFor(fake => fake.Description, fake => fake.Lorem.Sentence(3))
                .RuleFor(fake => fake.CreatedAt, fake => fake.Date.Past().ToUniversalTime())
                .RuleFor(fake => fake.UpdatedAt, fake => fake.Date.Past().ToUniversalTime());
        }

        public MemberPaymentEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
