using System;
using System.Globalization;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    internal class DateTimeTypeDecider : DecideTypesForStrings
    {
        public DateTimeTypeDecider(): base(TypeCompatibilityGroup.Exclusive, typeof(DateTime))
        {
        }

        protected override TypeDeciderResult IsAcceptableAsTypeImpl(string candidateString)
        {
            try
            {
                DateTime t;
                return new TypeDeciderResult(DateTime.TryParse(candidateString, CultureInfo.CurrentCulture, DateTimeStyles.NoCurrentDateDefault, out t));
            }
            catch (Exception)
            {
                return new TypeDeciderResult(false);
            }
        }
    }
}