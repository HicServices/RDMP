using System;
using Common.Logging;
using Common.Logging.Factory;

namespace ReusableLibraryCode.Progress
{
    /// <summary>
    /// Converts an IDataLoadEventListener into an Common.Logging.ILog for when you want to pass your event handler (IDataLoadEventListener) class on to other
    /// libraries which expect a normal Common.Logging.ILog object.  
    /// 
    /// Once constructed this class will respond to message logging calls by translating the LogLevel to a ProgressEventType and passing it to the 
    /// IDataLoadEventListener it was constructed with.
    /// </summary>
    public class FromDataLoadEventListenerToILog : AbstractLogger
    {
        private readonly object _sender;
        private readonly IDataLoadEventListener _listener;
        private readonly bool _showDebug;

        public FromDataLoadEventListenerToILog(object sender, IDataLoadEventListener listener, bool showDebug = true)
        {
            _sender = sender;
            _listener = listener;
            _showDebug = showDebug;
        }
        
        protected override void WriteInternal(LogLevel level, object message, Exception exception)
        {
            switch (level)
            {
                case LogLevel.All:
                    _listener.OnNotify(_sender, new NotifyEventArgs(ProgressEventType.Information, message.ToString(), exception));
                    break;
                case LogLevel.Trace:
                    _listener.OnNotify(_sender, new NotifyEventArgs(ProgressEventType.Information, message.ToString(), exception));
                    break;
                case LogLevel.Debug:
                    _listener.OnNotify(_sender, new NotifyEventArgs(ProgressEventType.Information, message.ToString(), exception));
                    break;
                case LogLevel.Info:
                    _listener.OnNotify(_sender, new NotifyEventArgs(ProgressEventType.Information, message.ToString(), exception));
                    break;
                case LogLevel.Warn:
                    _listener.OnNotify(_sender, new NotifyEventArgs(ProgressEventType.Warning, message.ToString(), exception));
                    break;
                case LogLevel.Error:
                    _listener.OnNotify(_sender, new NotifyEventArgs(ProgressEventType.Error, message.ToString(), exception));
                    break;
                case LogLevel.Fatal:
                    _listener.OnNotify(_sender, new NotifyEventArgs(ProgressEventType.Error, message.ToString(), exception));
                    break;
                case LogLevel.Off:
                    //ignored
                    break;
                default:
                    throw new ArgumentOutOfRangeException("level");
            }
        }

        public override bool IsTraceEnabled
        {
            get { return _showDebug; }
        }

        public override bool IsDebugEnabled
        {
            get { return _showDebug; }
        }

        public override bool IsErrorEnabled
        {
            get { return true; }
        }

        public override bool IsFatalEnabled
        {
            get { return true; }
        }

        public override bool IsInfoEnabled
        {
            get { return true; }
        }

        public override bool IsWarnEnabled
        {
            get { return true; }
        }
    }
}