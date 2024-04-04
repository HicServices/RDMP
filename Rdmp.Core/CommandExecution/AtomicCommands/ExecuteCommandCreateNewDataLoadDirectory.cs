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
public class ExecuteCommandCreateNewDataLoadDirectory : BasicCommandExecution
{
    /// <summary>
    /// The load if any to create the folder structure for
    /// </summary>
    public LoadMetadata LoadMetadata { get; }

    /// <summary>
    /// The directory to create or null to do the operation
    /// interactively.
    /// </summary>
    public DirectoryInfo Dir { get; }

    public ExecuteCommandCreateNewDataLoadDirectory(IBasicActivateItems activator,
        [DemandsInitialization(
            "Optional load for which you are creating the folder structure.  Will have its LocationOfFlatFiles set to the new dir if passed")]
        LoadMetadata load,
        [DemandsInitialization("The directory to create new load folders in.")]
        DirectoryInfo dir) : base(activator)
    {
        LoadMetadata = load;
        Dir = dir;
    }

    public override void Execute()
    {
        base.Execute();

        var d = Dir;
        string newFolderName = null;

        // if called with an explicit full dir then that is where we create load folders
        // otherwise get them to pick something that exists and then name a new folder to
        // create

        if (d == null)
        {
            d = BasicActivator.SelectDirectory("Directory to create in");

            if (d == null)
                return;

            if (!BasicActivator.TypeText("New Folder Name", "Name", 255, null, out newFolderName, false)) return;
        }

        var loadDir =
            string.IsNullOrWhiteSpace(newFolderName)
                ? LoadDirectory.CreateDirectoryStructure(d.Parent, d.Name, true)
                : LoadDirectory.CreateDirectoryStructure(d, newFolderName, true);

        // if we have a load then update the path to this location we just created
        if (LoadMetadata != null)
        {
            LoadMetadata.LocationOfForLoadingDirectory = loadDir.RootPath.FullName + "/ForLoading";
            LoadMetadata.LocationOfForArchivingDirectory = loadDir.RootPath.FullName + "/ForArchiving";
            LoadMetadata.LocationOfExecutablesDirectory = loadDir.RootPath.FullName + "/Executables";
            LoadMetadata.LocationOfCacheDirectory = loadDir.RootPath.FullName + "/Cache";
            LoadMetadata.SaveToDatabase();
            Publish(LoadMetadata);
        }
    }
}