using System.Xml.Serialization;

namespace HIC.Common.Validation.Constraints.Primary
{
    /// <summary>
    /// Each column can have a single PrimaryConstraint, this is usually related to the datatype (either exact e.g. DateTime or semantic e.g. NHS number).
    /// Validation of a PrimaryConstraint involves ensuring that the value is of the correct pattern/type as the concept.
    /// </summary>
    public interface IPrimaryConstraint : IConstraint
    {
        ValidationFailure Validate(object value);
    }
}