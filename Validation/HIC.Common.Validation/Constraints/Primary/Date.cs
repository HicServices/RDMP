using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HIC.Common.Validation.Constraints.Primary
{
    /// <summary>
    /// A Constraint specifying that the date must be a valid, delimited EU format date. e.g. 25-09-67 or 25-9-1967.
    /// As such, the leftmost digits are assumed to be the DAY value and the rightmost digits, the YEAR value.
    /// </summary>
    public class Date : PrimaryConstraint
    {
        private const string RegExp = @"^(\d{1,2})(\.|/|-)(\d{1,2})(\.|/|-)(\d{2}(\d{2})?)$";

        private readonly CultureInfo _ukCulture;

        public Date()
        {
            _ukCulture = new CultureInfo("en-GB");
        }
        
        /// <summary>
        /// Validate a string representation of a UK (ONLY) date of the format d[d]/m[m]/yy[yy].
        /// The standard C# DateTime.Parse() method is used, which accepts alternative separators such as '.' and '-'.
        /// </summary>
        /// <param name="value"></param>
        public override ValidationFailure Validate(object value)
        {
            if (value is DateTime)
                return null;

            if (value == null)
                return null;
            
            try
            {
                var s = (string)value;
                DateTime.Parse(s, _ukCulture.DateTimeFormat);

                if (NotAFullySpecifiedDate(s)) 
                    return new ValidationFailure("Partial dates not allowed.",this);
            }
            catch (FormatException ex)
            {
                return new ValidationFailure(ex.Message,this);
            }

            return null;
        }

        private static bool NotAFullySpecifiedDate(string s)
        {
            var match = Regex.Match(s, RegExp);
            return !match.Success;
        }


        public override void RenameColumn(string originalName, string newName)
        {
            
        }

        public override string GetHumanReadableDescriptionOfValidation()
        {
            return "Checks that the data type is DateTime or a string which can be parsed into a valid DateTime";
        }
    }
}
