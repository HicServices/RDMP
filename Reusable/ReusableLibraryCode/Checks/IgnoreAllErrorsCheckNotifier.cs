namespace ReusableLibraryCode.Checks
{
    /// <summary>
    /// ICheckNotifier which ignores all check messages completely (including failures) but responds true to any ProposedFixes.  Use this ICheckNotifier
    /// when you want to run the Check method on an ICheckable but don't care whether it passes or not.
    /// </summary>
    public class IgnoreAllErrorsCheckNotifier : ICheckNotifier
    {
        public bool OnCheckPerformed(CheckEventArgs args)
        {
            return true;
        }
    }
}