using ReusableLibraryCode.Progress;

namespace HIC.Logging.Listeners
{
    public static class NlogLoggerExtensions
    {
        public static string GetLogLevel(this ProgressEventType pet)
        {
            switch (pet)
            {
                case ProgressEventType.Trace:
                    return "Trace";
                case ProgressEventType.Debug:
                    return "Debug";
                case ProgressEventType.Information:
                    return "Info";
                case ProgressEventType.Warning:
                    return "Warn";
                case ProgressEventType.Error:
                    return "Error";
                default:
                    return "Off";
            }
        }
    }
}