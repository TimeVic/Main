using TimeTracker.Business.Common.Mvc.Attribute;

namespace TimeTracker.Business.Common.Constants
{
    public abstract class AConstant<T>: IValidationAttribute
    {
        protected readonly string _Name;
        protected readonly string _Value;

        public AConstant() { }

        protected AConstant(string value, string name)
        {
            _Name = name;
            _Value = value;
        }

        public override string ToString()
        {
            return _Name;
        }

        public string GetValue()
        {
            return _Value;
        }

        public override bool Equals(object Value)
        {
            var status = Value as AConstant<T>;
            return status?.GetValue().Equals(_Value) ?? false;
        }

        public override int GetHashCode()
        {
            return _Value.GetHashCode();
        }

        public abstract bool IsValid(string Value);
        public abstract T FromString(string Value);
    }
}
