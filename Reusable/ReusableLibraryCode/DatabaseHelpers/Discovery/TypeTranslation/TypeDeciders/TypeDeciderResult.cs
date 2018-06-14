namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders
{
    struct TypeDeciderResult
    {
        public bool IsCompatible;
        public int? NumbersBeforeDecimalPlace;
        public int? NumbersAfterDecimalPlace;

        public TypeDeciderResult(bool isCompatible)
        {
            IsCompatible = isCompatible;
            NumbersBeforeDecimalPlace = null;
            NumbersAfterDecimalPlace = null;
        }

        public TypeDeciderResult(bool isCompatible, int numbersBeforeDecimalPlace)
        {
            IsCompatible = isCompatible;
            NumbersBeforeDecimalPlace = numbersBeforeDecimalPlace;
            NumbersAfterDecimalPlace = null;
        }

        public TypeDeciderResult(bool isCompatible,int numbersBeforeDecimalPlace, int numbersAfterDecimalPlace)
        {
            IsCompatible = isCompatible;
            NumbersBeforeDecimalPlace = numbersBeforeDecimalPlace;
            NumbersAfterDecimalPlace = numbersAfterDecimalPlace;
        }
    }
}
