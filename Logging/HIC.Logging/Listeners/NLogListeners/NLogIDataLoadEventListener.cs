using HIC.Logging.Listeners.Extensions;
using NLog;
using ReusableLibraryCode.Progress;

namespace HIC.Logging.Listeners.NLogListeners
{
    /// <summary>
    /// <see cref="IDataLoadEventListener"/> that passes all events to an <see cref="NLog.LogManager"/>.  Optionally throws on Errors (after logging).
    /// </summary>
    public class NLogIDataLoadEventListener : NLogListener,IDataLoadEventListener
    {
        public NLogIDataLoadEventListener(bool throwOnError):base(throwOnError)
        {
            
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            base.Log(sender,e.ToLogLevel(), e.Exception, e.Message);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            base.Log(sender, LogLevel.Trace, null,
                string.Format("Progress: {0} {1}{2}", e.Progress.Value, e.Progress.UnitOfMeasurement, e.Progress.KnownTargetValue == 0 ? "" : " of " + e.Progress.KnownTargetValue)
                );
        }
    }
}