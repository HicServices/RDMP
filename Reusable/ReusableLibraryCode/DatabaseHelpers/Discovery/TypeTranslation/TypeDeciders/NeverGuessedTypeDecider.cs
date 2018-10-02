using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    /// <summary>
    /// DecideTypesForStrings for types that we should never assign to strings but need to support for CurrentEstimate
    /// </summary>
    class NeverGuessTheseTypeDecider : DecideTypesForStrings
    {
        public NeverGuessTheseTypeDecider() : base(TypeCompatibilityGroup.Exclusive, typeof(byte[]), typeof(Guid))
        {
        }

        protected override object ParseImpl(string value)
        {
            throw new NotSupportedException();
        }

        protected override bool IsAcceptableAsTypeImpl(string candidateString, DecimalSize sizeRecord)
        {
            //strings should never be interpreted as byte arrays
            return false;
        }
    }
}