// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NLog;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.Startup;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Deletes duplicate dlls within the given plugin.  RDMP will not load 2 copies of the same
/// dll at runtime and so these dlls will just bloat your plugin.  Use this command to prune out
/// those files.
/// </summary>
public class ExecuteCommandPrunePlugin : BasicCommandExecution
{
    private string file;


    [UseWithObjectConstructor]
    public ExecuteCommandPrunePlugin(string file)
    {
        this.file = file;
    }

    /// <summary>
    /// Interactive constructor
    /// </summary>
    /// <param name="activator"></param>
    public ExecuteCommandPrunePlugin(IBasicActivateItems activator) : base(activator)
    {

    }



    public override void Execute()
    {
        base.Execute();

        // make runtime decision about the file to run on
        if(file == null && BasicActivator != null)
        {
            file = BasicActivator.SelectFile("Select plugin to prune")?.FullName;
        }

        if(file == null)
        {
            return;
        }

        var logger = LogManager.GetCurrentClassLogger();

        using (var zf = ZipFile.Open(file, ZipArchiveMode.Update))
        {
            var current = UsefulStuff.GetExecutableDirectory();

            logger.Info($"Reading RDMP core dlls in directory '{current}'");

            var rdmpCoreFiles = current.GetFiles("*.dll");

            string main = $"^/?lib/{EnvironmentInfo.MainSubDir}/.*.dll";
            string windows = $"^/?lib/{EnvironmentInfo.WindowsSubDir}/.*.dll";

            var inMain = new List<ZipArchiveEntry>();
            var inWindows = new List<ZipArchiveEntry>();

            foreach (var e in zf.Entries.ToArray())
            {
                if (rdmpCoreFiles.Any(f => f.Name.Equals(e.Name)))
                {
                    logger.Info($"Deleting '{e.FullName}'");
                    e.Delete();
                    continue;
                }

                if (Regex.IsMatch(e.FullName, main))
                {
                    inMain.Add(e);
                }
                else if (Regex.IsMatch(e.FullName, windows))
                {
                    inWindows.Add(e);
                }
            }

            foreach (var dup in inWindows.Where(e => inMain.Any(o => o.Name.Equals(e.Name))).ToArray())
            {
                logger.Info($"Deleting '{dup.FullName}' because it is already in 'main' subdir");
                dup.Delete();
            }
        }

        if(BasicActivator != null)
        {
            BasicActivator.Show("Prune Completed");
        }
    }
}