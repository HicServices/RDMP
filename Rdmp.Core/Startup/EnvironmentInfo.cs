// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Startup;

[Flags]
public enum PluginFolders
{
    None = 0,
    Main = 1,
    Windows = 4
}

/// <summary>
///     Class for describing the runtime environment in which <see cref="Startup" /> is executing e.g. under
///     Windows / Linux in net461 or netcoreapp2.2.  This determines which plugin binary files are loaded
/// </summary>
public class EnvironmentInfo
{
    public const string MainSubDir = "main";
    public const string WindowsSubDir = "windows";

    /// <summary>
    ///     Flags indicating which plugins versions to load, if any.
    /// </summary>
    private readonly PluginFolders _pluginsToLoad;

    /// <summary>
    ///     Creates a new instance, optionally specifying which plugins should be loaded, default none.
    /// </summary>
    public EnvironmentInfo(PluginFolders pluginsToLoad = PluginFolders.None)
    {
        _pluginsToLoad = pluginsToLoad;
    }

    public static bool IsLinux
    {
        get
        {
            var p = (int)Environment.OSVersion.Platform;
            return p == 4 || p == 6 || p == 128;
        }
    }

    /// <summary>
    ///     Returns the nupkg archive subdirectory that should be loaded with the current environment
    ///     e.g. /lib/net461
    /// </summary>
    internal IEnumerable<DirectoryInfo> GetPluginSubDirectories(DirectoryInfo root, ICheckNotifier notifier)
    {
        if (!root.Name.Equals("lib"))
            throw new ArgumentException($"Expected {root.FullName} to be the 'lib' directory");

        // if we are loading the main codebase of plugins
        if (_pluginsToLoad.HasFlag(PluginFolders.Main))
        {
            // find the main dir
            var mainDir = root.GetDirectories(MainSubDir,
                    new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive, AttributesToSkip = 0 })
                .FirstOrDefault();

            if (mainDir != null)
                // great, go load the dlls in there
                yield return mainDir;
            else
                // plugin has no main directory, maybe it is not built correctly
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Could not find an expected folder called '/lib/{MainSubDir}' in folder:{root}",
                    CheckResult.Warning));
        }

        // if we are to load the windows specific (e.g. winforms) plugins too?
        if (_pluginsToLoad.HasFlag(PluginFolders.Windows))
        {
            // see if current plugin has winforms stuff
            var winDir = root.GetDirectories(WindowsSubDir, new EnumerationOptions
            {
                MatchCasing = MatchCasing.PlatformDefault,
                AttributesToSkip = 0
            }).FirstOrDefault();

            if (winDir != null)
                //yes
                yield return winDir;

            // if not then no big deal
        }
    }
}