using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    class IntTypeDecider : DecideTypesForStrings
    {
        public IntTypeDecider() : base(TypeCompatibilityGroup.Numerical, typeof(Int16) , typeof(Int32), typeof(Int64))
        {
        }

        protected override TypeDeciderResult IsAcceptableAsTypeImpl(string candidateString)
        {
            try
            {
                var t = Convert.ToInt32(candidateString);
                return new TypeDeciderResult(true,t.ToString().Length);
            }
            catch (Exception)
            {
                return new TypeDeciderResult(false);
            }
        }
    }
}