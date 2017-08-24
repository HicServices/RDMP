using System;

namespace ReusableLibraryCode.Progress
{
    public class ProgressEventArgs
    {
        public string TaskDescription { get; set; }
        public ProgressMeasurement Progress { get; set; }
        public TimeSpan TimeSpentProcessingSoFar { get; set; }

        public bool Handled { get; set; }

        public ProgressEventArgs(string taskDescription, ProgressMeasurement progress, TimeSpan timeSpentProcessingSoFar)
        {
            TaskDescription = taskDescription;
            Progress = progress;
            TimeSpentProcessingSoFar = timeSpentProcessingSoFar;

            Handled = false;
        }
    }

    public delegate void ProgressEventHandler(object sender, ProgressEventArgs args);
}