using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    class IntTypeDecider : DecideTypesForStrings
    {
        public IntTypeDecider() : base(TypeCompatibilityGroup.Numerical, typeof(Int16) , typeof(Int32), typeof(Int64))
        {
        }

        protected override bool IsAcceptableAsTypeImpl(string candidateString, DecimalSize sizeRecord)
        {
            try
            {
                var t = Convert.ToInt32(candidateString);
                
                sizeRecord.IncreaseTo(t.ToString().Length);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }
        }
    }
}