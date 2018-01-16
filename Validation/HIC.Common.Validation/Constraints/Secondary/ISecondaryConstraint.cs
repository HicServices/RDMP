using System.Xml.Serialization;
using HIC.Common.Validation.Constraints.Secondary.Predictor;
using HIC.Common.Validation.UIAttributes;

namespace HIC.Common.Validation.Constraints.Secondary
{
    /// <summary>
    /// Each column can have as many SecondaryConstraints as you want.  Each SecondaryConstraint is a general rule about the data that the column is allowed to
    /// contain.  This can include Regexes, NotNull requirements etc.
    /// </summary>
    public interface ISecondaryConstraint : IConstraint
    {
        /// <summary>
        /// Inherit this method to perform validation operations unique to your Class.  Column value could be DateTime, string or numerical.  Part of validation is 
        /// ensuring it is of the appropriate type.  otherColumns can be used for example in the case that you intend to predict something such as Gender from Title.
        /// If your validation fails you should return a ValidationFailure.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="otherColumns"></param>
        /// <param name="otherColumnNames"></param>
         ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames);

        [ExpectsLotsOfText]
        string Rationale { get; set; }
    }
}
