using System;
using System.Globalization;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    internal class TimeSpanTypeDecider : DecideTypesForStrings
    {
        public TimeSpanTypeDecider(): base(TypeCompatibilityGroup.Exclusive, typeof(TimeSpan))
        {
        }

        protected override bool IsAcceptableAsTypeImpl(string candidateString,DecimalSize sizeRecord)
        {
            try
            {
                DateTime t;

                //if it parses as a date 
                if (DateTime.TryParse(candidateString, CultureInfo.CurrentCulture, DateTimeStyles.NoCurrentDateDefault, out t))
                {
                    return t.Year == 1 && t.Month == 1 && t.Day == 1;//without any ymd component then it's a date...  this means 00:00 is a valid TimeSpan too 
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}