namespace ReusableLibraryCode.Checks
{
    public class IgnoreAllErrorsCheckNotifier : ICheckNotifier
    {
        public bool OnCheckPerformed(CheckEventArgs args)
        {
            return true;
        }
    }
}