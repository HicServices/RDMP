using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    class BoolTypeDecider: DecideTypesForStrings
    {
        public BoolTypeDecider(): base(TypeCompatibilityGroup.Numerical,typeof(bool))
        {
        }

        protected override TypeDeciderResult IsAcceptableAsTypeImpl(string candidateString)
        {
            bool result;

            return new TypeDeciderResult(bool.TryParse(candidateString, out result));
        }
    }
}