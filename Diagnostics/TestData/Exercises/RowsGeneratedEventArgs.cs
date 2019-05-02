using System;

namespace Diagnostics.TestData.Exercises
{
    public class RowsGeneratedEventArgs
    {
        public int RowsWritten { get; private set; }
        public TimeSpan ElapsedTime { get; private set; }
        public bool IsFinished { get; private set; }

        public RowsGeneratedEventArgs(int rowsWritten, TimeSpan elapsedTime, bool isFinished)
        {
            RowsWritten = rowsWritten;
            ElapsedTime = elapsedTime;
            IsFinished = isFinished;
        }
    }
}