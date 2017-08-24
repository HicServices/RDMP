using System;
using System.Text.RegularExpressions;

namespace HIC.Common.Validation.Constraints.Primary
{
    public class Alpha : PrimaryConstraint
    {
        public const string RegExp = @"^[A-Za-z]+$";

        public override ValidationFailure Validate(object value)
        {
            if (value == null)
                return null;
            
            var text = (string)value;
            var match = Regex.Match(text, RegExp);

            if (!match.Success)
            {
                return new ValidationFailure("Value [" + value + "] contains characters other than alphabetic",this);
            }

            return null;
        }

        public override void RenameColumn(string originalName, string newName)
        {
            
        }

        public override string GetHumanReadableDescriptionOfValidation()
        {
            return "Checks to see if input strings contain nothing but characters by using pattern " + RegExp;
        }

    }
}
