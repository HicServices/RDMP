// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NLog;
using Rdmp.Core.ReusableLibraryCode.Settings;
using System;
using System.IO;

namespace Rdmp.Core.Logging;


/// <summary>
/// Singleton logger for writing logs to file
/// </summary>
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
        logfile.Dispose();
    }


    public void LogEventToFile(string logType, object[] logItems)
    {
        var logMessage = $"{string.Join("|", Array.ConvertAll(logItems, item => item.ToString()))}";
        var Logger = NLog.LogManager.GetLogger(logType);
        Logger.Info(logMessage);
    }


}