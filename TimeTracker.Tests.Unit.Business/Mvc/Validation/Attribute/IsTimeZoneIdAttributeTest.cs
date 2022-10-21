using AutoMapper.Configuration;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;
using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Unit.Business.Mvc.Validation.Attribute
{
    public class IsTimeZoneIdAttributeTest
    {
        private readonly IsTimeZoneIdAttribute _validationAttribute;

        public IsTimeZoneIdAttributeTest()
        {
            _validationAttribute =  new IsTimeZoneIdAttribute();
        }

        [Theory]
        [InlineData("UTC")]
        [InlineData("utc")]
        [InlineData("Aleutian Standard Time")]
        public void IsValid(string timeZoneId)
        {
            Assert.True(_validationAttribute.IsValid(timeZoneId));
        }
        
        [Theory]
        [InlineData("fakeUtc")]
        public void IsInvalid(string timeZoneId)
        {
            Assert.False(_validationAttribute.IsValid(timeZoneId));
        }
    }
}
