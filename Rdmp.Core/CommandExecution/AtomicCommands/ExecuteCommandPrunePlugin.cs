// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using NLog;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Deletes duplicate dlls within the given plugin.  RDMP will not load 2 copies of the same
///     dll at runtime and so these dlls will just bloat your plugin.  Use this command to prune out
///     those files.
/// </summary>
public sealed partial class ExecuteCommandPrunePlugin : BasicCommandExecution
{
    private string _file;


    [UseWithObjectConstructor]
    public ExecuteCommandPrunePlugin(string file)
    {
        _file = file;
    }

    /// <summary>
    ///     Interactive constructor
    /// </summary>
    /// <param name="activator"></param>
    public ExecuteCommandPrunePlugin(IBasicActivateItems activator) : base(activator)
    {
    }


    public override void Execute()
    {
        base.Execute();

        // make runtime decision about the file to run on
        _file ??= BasicActivator?.SelectFile("Select plugin to prune")?.FullName;

        if (_file == null)
            return;

        var logger = LogManager.GetCurrentClassLogger();

        var main = MainRegex();
        var windows = WinRegex();
        var keep = KeepRegex();
        AssemblyLoadContext context = new(nameof(ExecuteCommandPrunePlugin), true);
        using (var zf = ZipFile.Open(_file, ZipArchiveMode.Update))
        {
            var rdmpCoreFiles = UsefulStuff.GetExecutableDirectory().GetFiles("*.dll");
            var inMain = new HashSet<string>();
            var inWindows = new List<ZipArchiveEntry>();

            foreach (var e in zf.Entries.ToArray())
            {
                // Purge anything but directories, DLLs and plugin metadata
                if (!keep.IsMatch(e.Name))
                {
                    logger.Info($"Deleting '{e.FullName}' (non-DLL)");
                    e.Delete();
                    continue;
                }

                // Now we filter the DLLs to keep only .NET assemblies, and de-duplicate those too
                if (!e.Name.EndsWith(".dll", StringComparison.Ordinal))
                    continue;

                Assembly assembly;
                if (SafeDirectoryCatalog.Ignore.Contains(e.Name.ToLowerInvariant()) ||
                    rdmpCoreFiles.Any(f => f.Name.Equals(e.Name)))
                {
                    logger.Info($"Deleting '{e.FullName}' (static)");
                    e.Delete();
                    continue;
                }

                try
                {
                    using var stream = e.Open();
                    assembly = context.LoadFromStream(stream);
                }
                catch (Exception exception)
                {
                    logger.Info(exception,
                        $"Deleting corrupt or non-.Net file {e.FullName} due to {exception.Message}");
                    e.Delete();
                    continue;
                }

                if (AssemblyLoadContext.Default.Assemblies.Any(a => a.FullName?.Equals(assembly.FullName) == true))
                {
                    logger.Info($"Deleting '{e.FullName}' (dynamic)");
                    e.Delete();
                    continue;
                }

                if (main.IsMatch(e.FullName))
                    inMain.Add(e.Name);
                else if (windows.IsMatch(e.FullName))
                    inWindows.Add(e);
                else
                    logger.Warn($"Unclassified plugin component {e.FullName}");
            }

            foreach (var dup in inWindows.Where(e => inMain.Contains(e.Name)))
            {
                logger.Info($"Deleting '{dup.FullName}' because it is already in 'main' subdir");
                dup.Delete();
            }

            context.Unload();
        }

        BasicActivator?.Show("Prune Completed");
    }

    [GeneratedRegex("/main/.*\\.dll$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MainRegex();

    [GeneratedRegex("/windows/.*\\.dll$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex WinRegex();

    [GeneratedRegex("(.nuspec|.dll|.rdmp|/)$", RegexOptions.CultureInvariant)]
    private static partial Regex KeepRegex();
}