namespace ReusableLibraryCode.Progress
{
    public class ProgressMeasurement
    {
        public int Value { get; set; }
        public ProgressType UnitOfMeasurement { get; set; }
        public int KnownTargetValue { get; set; }

        public ProgressMeasurement(int value, ProgressType unit)
        {
            Value = value;
            UnitOfMeasurement = unit;
        }
        public ProgressMeasurement(int value, ProgressType unit, int knownTargetValue)
        {
            Value = value;
            UnitOfMeasurement = unit;
            KnownTargetValue = knownTargetValue;
        }
    }
}