using System.Diagnostics;
using ReusableLibraryCode.Progress;
using Common.Logging;

namespace CachingService
{
    public class CommonLoggingListener : IDataLoadEventListener
    {
        public void OnNotify(object sender, NotifyEventArgs e)
        {
            var logger = LogManager.GetLogger(sender.GetType());
            switch (e.ProgressEventType)
            {
                case ProgressEventType.Information:
                    logger.Info(e.Message, e.Exception);
                    break;
                case ProgressEventType.Warning:
                    logger.Warn(e.Message, e.Exception);
                    break;
                case ProgressEventType.Error:
                    logger.Error(e.Message, e.Exception);
                    break;
                default:
                    logger.Info(e.Message, e.Exception);
                    break;
            }
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
        }
    }
}