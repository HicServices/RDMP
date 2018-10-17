using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.ExtractionTime.Listeners
{

    /// <summary>
    /// A simple DataLoadEventListener to be used during extraction so that the state can be moved to "crashed" if any component raises an error without an exception.
    /// </summary>
    public class ElevateStateListener : IDataLoadEventListener
    {
        private readonly ExtractCommand extractCommand;

        public ElevateStateListener(ExtractCommand extractCommand)
        {
            this.extractCommand = extractCommand;
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            if (e.ProgressEventType == ProgressEventType.Error)
                extractCommand.ElevateState(ExtractCommandState.Crashed);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
        }
    }
}