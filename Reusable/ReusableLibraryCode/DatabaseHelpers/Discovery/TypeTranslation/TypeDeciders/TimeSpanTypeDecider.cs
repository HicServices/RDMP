using System;
using System.Globalization;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    internal class TimeSpanTypeDecider : DecideTypesForStrings
    {
        public TimeSpanTypeDecider(): base(TypeCompatibilityGroup.Exclusive, typeof(TimeSpan))
        {
        }

        protected override TypeDeciderResult IsAcceptableAsTypeImpl(string candidateString)
        {
            try
            {
                DateTime t;

                //if it parses as a date 
                if (DateTime.TryParse(candidateString, CultureInfo.CurrentCulture, DateTimeStyles.NoCurrentDateDefault, out t))
                {
                    return new TypeDeciderResult((t.Year == 1 && t.Month == 1 && t.Day == 1));//without any ymd component then it's a date...  this means 00:00 is a valid TimeSpan too 
                }

                return new TypeDeciderResult(false);
            }
            catch (Exception)
            {
                return new TypeDeciderResult(false);
            }
        }
    }
}