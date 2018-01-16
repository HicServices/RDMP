namespace ReusableLibraryCode.Checks
{
    /// <summary>
    /// ICheckable where the Check method simply replays an existing list of CheckEventArgs stored in a ToMemoryCheckNotifier.  The use case for this is when
    /// you want to store the results of checking an ICheckable then replay it later (possibly multiple times) e.g. into a UI component.
    /// </summary>
    public class ReplayCheckable : ICheckable
    {
        private readonly ToMemoryCheckNotifier _toReplay;

        public ReplayCheckable(ToMemoryCheckNotifier toReplay)
        {
            _toReplay = toReplay;
        }

        public void Check(ICheckNotifier notifier)
        {
            foreach (var msg in _toReplay.Messages)
            {
                //don't propose fixes since this is a replay the time for applying the fix has long expired.
                msg.ProposedFix = null;

                notifier.OnCheckPerformed(msg);
            }
        }

    }
}