namespace ReusableLibraryCode.Checks
{
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