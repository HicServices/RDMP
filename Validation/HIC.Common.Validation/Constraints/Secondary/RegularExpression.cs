using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using HIC.Common.Validation.UIAttributes;

namespace HIC.Common.Validation.Constraints.Secondary
{
    public class RegularExpression : SecondaryConstraint
    {
        private string _pattern;
        
        public RegularExpression()
        {
            // Required for serialisation. 
        }

        public RegularExpression(string pattern)
        {
            _pattern = pattern;
        }

        [Description("The Regular Expression pattern that MUST match the value being validated.  If you find yourself copy pasting the same Pattern all over the place you should instead consider a StandardRegexConstraint")]
        [ExpectsLotsOfText]
        public string Pattern
        {
            get
            {
                return _pattern;
            }
            set
            {
                //throws if you pass an invalid pattern
                new Regex(value);

                
                _pattern = value;
            }
        }

         public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
        {
            if (value == null)
                return null;

            //if it is a basic type e.g. user wants to validate using regex [0-9]? (single digit!) then let them
            if (value is string == false)
                value = Convert.ToString(value);

            var text = (string)value;
            Match match = Regex.Match(text, _pattern);

            if (!match.Success) 
                return new ValidationFailure("Failed to match text [" + value + "] to regular expression /"+_pattern+"/",this);

            return null;
        }

        public override void RenameColumn(string originalName, string newName)
        {
            
        }

        public override string GetHumanReadableDescriptionOfValidation()
        {
            return "Matches regex " + Pattern;
        }
    }
}
