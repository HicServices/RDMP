// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewProcessTask : BasicUICommandExecution, IAtomicCommand
    {
        private readonly ProcessTaskType _taskType;
        private readonly LoadMetadata _loadMetadata;
        private readonly LoadStage _loadStage;
        private Bitmap _image;
        private LoadDirectory _LoadDirectory;
        private FileInfo _file;

        public ExecuteCommandCreateNewProcessTask(IActivateItems activator, ProcessTaskType taskType, LoadMetadata loadMetadata, LoadStage loadStage, FileInfo file=null) : base(activator)
        {
            _taskType = taskType;
            _loadMetadata = loadMetadata;
            _loadStage = loadStage;

            try
            {
                _LoadDirectory = new LoadDirectory(_loadMetadata.LocationOfFlatFiles);
            }
            catch (Exception)
            {
                SetImpossible("Could not construct LoadDirectory");
            }
            
            if(taskType == ProcessTaskType.SQLFile)
            {
                _image = activator.CoreIconProvider.GetImage(RDMPConcept.SQL, OverlayKind.Add);
            }
            else if(taskType == ProcessTaskType.Executable)
            {
                _image = new IconOverlayProvider().GetOverlayNoCache(CatalogueIcons.Exe, OverlayKind.Add);
            }
            else 
                SetImpossible("Only SQLFile and Executable task types are supported by this command");

            if (!ProcessTask.IsCompatibleStage(taskType, loadStage))
                SetImpossible("You cannot run "+taskType+" in " + loadStage);

            _file = file;
        }

        public override void Execute()
        {
            base.Execute();

            if (_file == null)
            {
                if (_taskType == ProcessTaskType.SQLFile)
                {
                        var dialog = new TypeTextOrCancelDialog("Enter a name for the SQL file", "File name", 100, "myscript.sql");
                        if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.ResultText))
                        {
                            var target = Path.Combine(_LoadDirectory.ExecutablesPath.FullName, dialog.ResultText);

                            if (!target.EndsWith(".sql"))
                                target += ".sql";

                            //create it if it doesn't exist
                            if (!File.Exists(target))
                                File.WriteAllText(target, "/*todo Type some SQL*/");

                            _file = new FileInfo(target);
                        }
                        else
                            return; //user cancelled
                }
                else if (_taskType == ProcessTaskType.Executable)
                {
                    var dialog = new OpenFileDialog();
                    dialog.Filter = "Executables|*.exe";
                    dialog.CheckFileExists = true;

                    if (dialog.ShowDialog() == DialogResult.OK)
                        _file = new FileInfo(dialog.FileName);
                    else
                        return;
                }
                else
                    throw new ArgumentOutOfRangeException("Unexpected _taskType:" + _taskType);
            }

            var task = new ProcessTask((ICatalogueRepository)_loadMetadata.Repository, _loadMetadata, _loadStage);
            task.ProcessTaskType = _taskType;
            task.Path = _file.FullName;
            SaveAndShow(task);
        }

        private void SaveAndShow(ProcessTask task)
        {
            task.Name = "Run '" + Path.GetFileName(task.Path) +"'";
            task.SaveToDatabase();

            Publish(_loadMetadata);
            Activate(task);
        }

        public override string GetCommandName()
        {
            switch (_taskType)
            {
                case ProcessTaskType.Executable:
                    return "Add Run .exe File Task";
                case ProcessTaskType.SQLFile:
                    return "Add Run SQL Script Task";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return _image;
        }
    }
}