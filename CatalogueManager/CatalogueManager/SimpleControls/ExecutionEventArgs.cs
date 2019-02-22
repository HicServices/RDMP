using System;

namespace CatalogueManager.SimpleControls
{
    public class ExecutionEventArgs:EventArgs
    {
        public int? ExitCode { get; set; }

        public ExecutionEventArgs(int exitCode)
        {
            ExitCode = exitCode;
        }
    }
}