using System;

namespace ReusableLibraryCode.Progress
{
    public class ThrowImmediatelyDataLoadEventListener : IDataLoadEventListener
    {
        /// <summary>
        /// By default this class will only throw Fail results but if you set this flag then it will also throw warning messages
        /// </summary>
        public bool ThrowOnWarning { get; set; }
        
        public bool WriteToConsole { get; set; }

        public ThrowImmediatelyDataLoadEventListener()
        {
            WriteToConsole = true;
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            if (WriteToConsole)
                Console.WriteLine(sender + ":" + e.Message);

            if(e.ProgressEventType == ProgressEventType.Error || 
                (e.ProgressEventType == ProgressEventType.Warning && ThrowOnWarning))
                throw new Exception(e.Message, e.Exception);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            
        }
    }
}
