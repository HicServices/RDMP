// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.ProcessTasks;

/// <summary>
/// Lets you view/edit an Executable file load task.  This exe will be run at the appropriate time in the data load (with the arguments displayed in the black box).
/// </summary>
public partial class ExeProcessTaskUI : ExeProcessTaskUI_Design
{
    private ProcessTask _processTask;
    private ExecutableRuntimeTask _runtimeTask;
    private Task _runTask;

    public ExeProcessTaskUI()
    {
        InitializeComponent();

        pbFile.Image = CatalogueIcons.Exe.ImageToBitmap();
        AssociatedCollection = RDMPCollection.DataLoad;
    }

    public override void SetDatabaseObject(IActivateItems activator, ProcessTask databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        _processTask = databaseObject;

        SetupForFile();

        CommonFunctionality.AddChecks(GetRuntimeTask);
        CommonFunctionality.StartChecking();
    }

    private void SetupForFile()
    {
        lblPath.Text = _processTask.Path;

        lblPath.Text = _processTask.Path;
        btnBrowse.Left = lblPath.Right;

        lblID.Left = btnBrowse.Right;
        tbID.Left = lblID.Right;

        tbID.Text = _processTask.ID.ToString();

        loadStageIconUI1.Setup(Activator.CoreIconProvider, _processTask.LoadStage);
        loadStageIconUI1.Left = tbID.Right + 2;
    }

    private ExecutableRuntimeTask GetRuntimeTask()
    {
        var lmd = _processTask.LoadMetadata;
        var argsDictionary = new LoadArgsDictionary(lmd, new HICDatabaseConfiguration(lmd).DeployInfo);

        //populate the UI with the args
        _runtimeTask =
            (ExecutableRuntimeTask)RuntimeTaskFactory.Create(_processTask,
                argsDictionary.LoadArgs[_processTask.LoadStage]);
        tbExeCommand.Text = $"{_runtimeTask.ExeFilepath} {_runtimeTask.CreateArgString()}";

        return _runtimeTask;
    }

    private void btnRunExe_Click(object sender, EventArgs e)
    {
        if (_runTask is { IsCompleted: false })
        {
            MessageBox.Show("Exe is still running");
            return;
        }

        if (_runtimeTask == null)
        {
            MessageBox.Show("Command could not be built,see Checks");
            return;
        }

        btnRunExe.Enabled = false;
        _runTask = new Task(() =>
        {
            try
            {
                _runtimeTask.Run(
                    new ThrowImmediatelyDataLoadJob(new FromCheckNotifierToDataLoadEventListener(checksUI1)),
                    new GracefulCancellationToken());
            }
            catch (Exception ex)
            {
                ExceptionViewer.Show(ex);
            }
            finally
            {
                Invoke(new MethodInvoker(() => btnRunExe.Enabled = true));
            }
        });
        _runTask.Start();
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Executables|*.exe",
            CheckFileExists = true
        };

        //open the browse dialog at the location of the currently specified file
        if (!string.IsNullOrWhiteSpace(_processTask.Path))
        {
            var fi = new FileInfo(_processTask.Path);
            if (fi.Exists && fi.Directory != null)
                dialog.InitialDirectory = fi.Directory.FullName;
        }

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _processTask.Path = dialog.FileName;
            _processTask.SaveToDatabase();
            Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_processTask));
            SetupForFile();
        }
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExeProcessTaskUI_Design, UserControl>))]
public abstract class ExeProcessTaskUI_Design : RDMPSingleDatabaseObjectControl<ProcessTask>;