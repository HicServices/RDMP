using System.Xml.Serialization;

namespace HIC.Common.Validation.Constraints.Primary
{
    public interface IPrimaryConstraint : IConstraint
    {
        ValidationFailure Validate(object value);
    }
}