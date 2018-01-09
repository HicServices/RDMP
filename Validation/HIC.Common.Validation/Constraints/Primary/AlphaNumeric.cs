using System;
using System.Text.RegularExpressions;

namespace HIC.Common.Validation.Constraints.Primary
{
    /// <summary>
    /// Field can contain only letters and numbers (but no spaces or symbols)
    /// </summary>
    public class AlphaNumeric : PrimaryConstraint
    {
        public const string RegExp = @"^[A-Za-z0-9]([A-Za-z0-9]*)$";
        
        public override ValidationFailure Validate(object value)
        {
            if (value == null)
                return null;

            var text = (string)value;
            var match = Regex.Match(text, RegExp);

            if (!match.Success)
                return new ValidationFailure("Value [" + value + "] contains characters other than alphanumeric",this);

            return null;
        }
        
        public override void RenameColumn(string originalName, string newName)
        {
            
        }

        public override string GetHumanReadableDescriptionOfValidation()
        {
            return
                "Checks that values have 1 or more characters/numbers in a sequence with no spaces or other punctuation";
        }
    }
}
