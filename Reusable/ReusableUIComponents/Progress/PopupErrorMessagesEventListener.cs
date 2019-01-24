using System;
using ReusableLibraryCode.Progress;
using ReusableUIComponents.Dialogs;

namespace ReusableUIComponents.Progress
{
    public class PopupErrorMessagesEventListener : IDataLoadEventListener
    {
        public void OnNotify(object sender, NotifyEventArgs e)
        {
            if(e.Exception != null)
                ExceptionViewer.Show(e.Exception);

            if(e.ProgressEventType == ProgressEventType.Error)
                WideMessageBox.Show(e.Message,"", Environment.StackTrace);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            
        }
    }
}
