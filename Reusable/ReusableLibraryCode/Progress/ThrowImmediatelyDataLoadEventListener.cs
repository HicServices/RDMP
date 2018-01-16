using System;

namespace ReusableLibraryCode.Progress
{
    /// <summary>
    /// IDataLoadEventListener that ignores all OnProgress messages but responds to OnNotify events of ProgressEventType.Error (and optionally Warning) by 
    /// raising an Exception.  Use this if you need an IDataLoadEventListener and don't care about the messages it sends (unless they are errors).
    /// </summary>
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
