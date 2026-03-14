using AutoMapper.Configuration;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;
using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Unit.Business.Mvc.Validation.Attribute
{
    public class IsSlugAttributeTest
    {
        private readonly IsSlugAttribute _validationAttribute;

        public IsSlugAttributeTest()
        {
            _validationAttribute =  new IsSlugAttribute();
        }

        [Theory]
        [InlineData("abc-abc")]
        [InlineData("utc")]
        public void IsValid(string slug)
        {
            Assert.True(_validationAttribute.IsValid(slug));
        }
        
        [Theory]
        [InlineData("Columbia")]
        [InlineData("bla bla")]
        public void IsInvalid(string slug)
        {
            Assert.False(_validationAttribute.IsValid(slug));
        }
    }
}
