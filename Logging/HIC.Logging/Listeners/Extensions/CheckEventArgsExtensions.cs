using System;
using NLog;
using ReusableLibraryCode.Checks;

namespace HIC.Logging.Listeners.Extensions
{
    public static class CheckEventArgsExtensions
    {
        public static LogLevel ToLogLevel(this CheckEventArgs args)
        {
            switch (args.Result)
            {
                case CheckResult.Success:
                    return LogLevel.Info;
                case CheckResult.Warning:
                    return LogLevel.Warn;
                case CheckResult.Fail:
                    return LogLevel.Error;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}