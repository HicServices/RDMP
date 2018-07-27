using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    class BoolTypeDecider: DecideTypesForStrings
    {
        public BoolTypeDecider(): base(TypeCompatibilityGroup.Numerical,typeof(bool))
        {
        }

        protected override object ParseImpl(string value)
        {
            return bool.Parse(value);
        }

        protected override bool IsAcceptableAsTypeImpl(string candidateString,DecimalSize sizeRecord)
        {
            bool result;

            return bool.TryParse(candidateString, out result);
        }
    }
}