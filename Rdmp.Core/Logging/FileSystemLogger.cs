using NLog;
using Rdmp.Core.ReusableLibraryCode.Settings;
using System;
using System.IO;

namespace Rdmp.Core.Logging;

public class FileSystemLogger
{

    private static readonly Lazy<FileSystemLogger> lazy =
         new Lazy<FileSystemLogger>(() => new FileSystemLogger());

    public static FileSystemLogger Instance { get { return lazy.Value; } }



    private FileSystemLogger()
    {
        var location = UserSettings.FileSystemLogLocation;
        var config = new NLog.Config.LoggingConfiguration();
        var logfile = new NLog.Targets.FileTarget("ProgressLog") { FileName = Path.Combine(location,"ProgressLog.log"), ArchiveAboveSize= UserSettings.LogFileSizeLimit };
        config.AddRule(LogLevel.Info, LogLevel.Info, logfile);
        NLog.LogManager.Configuration = config;

    }


    public void LogEventToFile(string logType, object[] logItems)
    {
        var logMessage = $"{string.Join("|", Array.ConvertAll(logItems, item => item.ToString()))}";
        var Logger = NLog.LogManager.GetLogger(logType);
        Logger.Info(logMessage);
    }


}