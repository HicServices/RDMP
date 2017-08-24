using System;

namespace ReusableLibraryCode.Progress
{
    public class ToConsoleDataLoadEventReceiver:IDataLoadEventListener
    {
        private readonly bool _throwOnErrorNotification;

        public ToConsoleDataLoadEventReceiver(bool throwOnErrorNotification = true)
        {
            _throwOnErrorNotification = throwOnErrorNotification;
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            if(e.ProgressEventType == ProgressEventType.Error && _throwOnErrorNotification)
                throw new Exception(e.Message, e.Exception);

            Console.WriteLine(sender + " sent message:" +e.Message);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            
        }
    }
}
