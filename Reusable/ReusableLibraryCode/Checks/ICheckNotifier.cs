namespace ReusableLibraryCode.Checks
{
    /// <summary>
    /// Class responsible for responding to checking successes/failures and ProposedFixes.  Event handler for CheckEventArgs.  This class
    /// should be passed to the Check method of an ICheckable.  See CheckEventArgs for the workflow.
    /// </summary>
    public interface ICheckNotifier
    {
        bool OnCheckPerformed(CheckEventArgs args);
    }
}