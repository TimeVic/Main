using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Tests.Unit.Business.Mvc.Validation.Attribute
{
    public class IsColorAttributeTest
    {
        private readonly IsColorAttribute _validationAttribute;

        public IsColorAttributeTest()
        {
            _validationAttribute =  new IsColorAttribute();
        }

        [Theory]
        [InlineData("#66fc03")]
        [InlineData("#1403fc")]
        public void IsValid(string colorHex)
        {
            Assert.True(_validationAttribute.IsValid(colorHex));
        }
        
        [Theory]
        [InlineData("123asd")]
        public void IsInvalid(string colorHex)
        {
            Assert.False(_validationAttribute.IsValid(colorHex));
        }
    }
}
