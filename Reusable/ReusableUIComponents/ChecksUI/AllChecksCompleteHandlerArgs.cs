using ReusableLibraryCode.Checks;

namespace ReusableUIComponents.ChecksUI
{
    public class AllChecksCompleteHandlerArgs
    {
        public ToMemoryCheckNotifier CheckResults { get; private set; }

        public AllChecksCompleteHandlerArgs(ToMemoryCheckNotifier checkResults)
        {
            CheckResults = checkResults;
        }
    }
}