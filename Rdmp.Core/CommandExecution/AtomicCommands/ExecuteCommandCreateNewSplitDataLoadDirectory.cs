// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Creates the expected and required flat file layout for a <see cref="LoadMetadata"/>
/// </summary>
public class ExecuteCommandCreateNewSplitDataLoadDirectory : BasicCommandExecution
{
    /// <summary>
    /// The load if any to create the folder structure for
    /// </summary>
    public LoadMetadata LoadMetadata { get; }

    /// <summary>
    /// The directory to create or null to do the operation
    /// interactively.
    /// </summary>
    public DirectoryInfo ForLoadingDir { get; }
    /// <summary>
    /// The directory to create or null to do the operation
    /// interactively.
    /// </summary>
    public DirectoryInfo ForArchivingDir { get; }
    /// <summary>
    /// The directory to create or null to do the operation
    /// interactively.
    /// </summary>
    public DirectoryInfo ExecutablesDir { get; }
    /// <summary>
    /// The directory to create or null to do the operation
    /// interactively.
    /// </summary>
    public DirectoryInfo CacheDir { get; }

    public ExecuteCommandCreateNewSplitDataLoadDirectory(IBasicActivateItems activator,
        [DemandsInitialization(
            "Optional load for which you are creating the folder structure.  Will have its LocationOfFlatFiles set to the new dir if passed")]
        LoadMetadata load,
        [DemandsInitialization("The directory to create new loads in.")]
        DirectoryInfo forLoadingDir,
         [DemandsInitialization("The directory to create new archives in.")]
        DirectoryInfo forArchivingDir,
          [DemandsInitialization("The directory to create new executables in.")]
        DirectoryInfo executablesDir,
           [DemandsInitialization("The directory to create new caches in.")]
        DirectoryInfo cacheDir
        ) : base(activator)
    {
        LoadMetadata = load;
        ForLoadingDir = forLoadingDir;
        ForArchivingDir = forArchivingDir;
        ExecutablesDir = executablesDir;
        CacheDir = cacheDir;
    }

    public override void Execute()
    {
        base.Execute();

        var fl = ForLoadingDir;
        var fa = ForArchivingDir;
        var e = ExecutablesDir;
        var c = CacheDir;

        // if called with an explicit full dir then that is where we create load folders
        // otherwise get them to pick something that exists and then name a new folder to
        // create

        if (fl == null)
        {
            fl = BasicActivator.SelectDirectory("Directory to load files in from");

            if (fl == null)
                return;

        }
        if (fa == null)
        {
            fa = BasicActivator.SelectDirectory("Directory to store the archives in");

            if (fa == null)
                return;

        }
        if (e== null)
        {
            e = BasicActivator.SelectDirectory("Directory to store executables");

            if (e == null)
                return;

        }
        if (c == null)
        {
            c = BasicActivator.SelectDirectory("Directory to store caches");

            if (c == null)
                return;

        }

        var loadDir = new LoadDirectory(fl.FullName, fa.FullName, e.FullName, c.FullName);

        // if we have a load then update the path to this location we just created
        if (LoadMetadata != null)
        {
            LoadMetadata.LocationOfForLoadingDirectory = loadDir.ForLoading.FullName;
            LoadMetadata.LocationOfForArchivingDirectory = loadDir.ForArchiving.FullName;
            LoadMetadata.LocationOfExecutablesDirectory = loadDir.ExecutablesPath.FullName;
            LoadMetadata.LocationOfCacheDirectory = loadDir.Cache.FullName;
            LoadMetadata.SaveToDatabase();
            Publish(LoadMetadata);
        }
    }
}