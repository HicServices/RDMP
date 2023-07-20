// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using SixLabors.ImageSharp;
using System.IO;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateNewFileBasedProcessTask : BasicCommandExecution
{
    private readonly ProcessTaskType _taskType;
    private readonly LoadMetadata _loadMetadata;
    private readonly LoadStage _loadStage;
    private readonly LoadDirectory _loadDirectory;
    private FileInfo _file;

    public ExecuteCommandCreateNewFileBasedProcessTask(IBasicActivateItems activator, ProcessTaskType taskType,
        LoadMetadata loadMetadata, LoadStage loadStage, FileInfo file = null) : base(activator)
    {
        _taskType = taskType;
        _loadMetadata = loadMetadata;
        _loadStage = loadStage;

        try
        {
            _loadDirectory = new LoadDirectory(_loadMetadata.LocationOfFlatFiles);
        }
        catch (Exception)
        {
            SetImpossible("Could not construct LoadDirectory");
        }
            
        if(taskType is not (ProcessTaskType.SQLFile or ProcessTaskType.Executable))
            SetImpossible("Only SQLFile and Executable task types are supported by this command");

        if (!ProcessTask.IsCompatibleStage(taskType, loadStage))
            SetImpossible($"You cannot run {taskType} in {loadStage}");

        _file = file;
    }

    public override void Execute()
    {
        base.Execute();

        if (_file == null)
        {
            if (_taskType == ProcessTaskType.SQLFile)
            {
                if (BasicActivator.TypeText("Enter a name for the SQL file", "File name", 100, "myscript.sql",
                        out var selected, false))
                {
                    var target = Path.Combine(_loadDirectory.ExecutablesPath.FullName, selected);

                    if (!target.EndsWith(".sql"))
                        target += ".sql";

                    //create it if it doesn't exist
                    if (!File.Exists(target))
                        File.WriteAllText(target, "/*todo Type some SQL*/");

                    _file = new FileInfo(target);
                }
                else
                {
                    return; //user cancelled
                }
            }
            else if (_taskType == ProcessTaskType.Executable)
            {
                _file = BasicActivator.SelectFile("Enter executable's path", "Executables", "*.exe");

                // they didn't pick one
                if (_file == null)
                    return;

                if (!_file.Exists)
                    throw new FileNotFoundException("File did not exist");
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Unexpected _taskType:{_taskType}");
            }
        }

        var task = new ProcessTask((ICatalogueRepository)_loadMetadata.Repository, _loadMetadata, _loadStage)
            {
                ProcessTaskType = _taskType,
                Path = _file.FullName
            };
        SaveAndShow(task);
    }

    private void SaveAndShow(ProcessTask task)
    {
        task.Name = $"Run '{Path.GetFileName(task.Path)}'";
        task.SaveToDatabase();

        Publish(_loadMetadata);
        Activate(task);
    }

    public override string GetCommandName()
    {
        return _taskType switch
        {
            ProcessTaskType.Executable => "Add Run .exe File Task",
            ProcessTaskType.SQLFile => "Add Run SQL Script Task",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return _taskType switch
        {
            ProcessTaskType.SQLFile => iconProvider.GetImage(RDMPConcept.SQL, OverlayKind.Add),
            ProcessTaskType.Executable => IconOverlayProvider.GetOverlayNoCache(
                Image.Load<Rgba32>(CatalogueIcons.Exe), OverlayKind.Add),
            _ => null
        };
    }
}