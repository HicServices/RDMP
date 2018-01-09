namespace ReusableLibraryCode.Progress
{
    /// <summary>
    /// Part of Event args for IDataLoadEventListener.OnProgress events.  Records how far through the operation the ProgressEventArgs is (how many records have
    /// been processed / how many kilobytes have been written etc).  You can include a knownTargetValue if you know how many records etc you need to process in 
    /// total or you can just keep incrementing the count without knowing the goal number.
    /// </summary>
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