using System;
using System.Globalization;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    internal class DateTimeTypeDecider : DecideTypesForStrings
    {
        private TimeSpanTypeDecider _timeSpanTypeDecider = new TimeSpanTypeDecider();
        private DecimalTypeDecider _decimalChecker = new DecimalTypeDecider();

        public DateTimeTypeDecider(): base(TypeCompatibilityGroup.Exclusive, typeof(DateTime))
        {
        }

        protected override TypeDeciderResult IsAcceptableAsTypeImpl(string candidateString)
        {
            //if it's a float then it isn't a date is it! thanks C# for thinking 1.1 is the first of January
            if (_decimalChecker.IsAcceptableAsType(candidateString).IsCompatible)
                return new TypeDeciderResult(false);

            //likewise if it is just the Time portion of the date then we have a column with mixed dates and times which SQL will not deal with well in the end database (e.g. it will set the
            //date portion of times to todays date which will be very confusing
            if (_timeSpanTypeDecider.IsAcceptableAsType(candidateString).IsCompatible)
                return new TypeDeciderResult(false);

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