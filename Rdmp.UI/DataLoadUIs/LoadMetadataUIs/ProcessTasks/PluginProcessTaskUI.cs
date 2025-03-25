// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PipelineUIs.DemandsInitializationUIs;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.ProcessTasks;

/// <summary>
///     Lets you view/edit a single data load module.  This is a pre-canned class e.g. FTPDownloader or a custom plugin you
///     have written.  You should ensure
///     that the Name field accurately describes (in plenty of detail) what the module/script is intended to do.
///     <para>
///         These can be either:
///         Attacher - Run the named C# class (which implements the interface IAttacher).  This only works in Mounting
///         stage.  This usually results in records being loaded into the RAW bubble (e.g. AnySeparatorFileAttacher)
///         DataProvider - Run the named C# class (which implements IDataProvider).  Normally this runs in GetFiles but
///         really it can run on any Stage.  This usually results in files being created or modified (e.g. FTPDownloader)
///         MutilateDataTable - Run the named C# class (which implements IMutilateDataTables).  Runs in any Adjust/PostLoad
///         stage.  These are dangerous operations which operate pre-canned functionality directly
///         on the DataTable being loaded e.g. resolving primary key collisions (which can result in significant data loss
///         if you have not configured the correct primary keys on your dataset).
///     </para>
///     <para>
///         Each C# module based task has a collection of arguments which each have a description of how they change the
///         behaviour of the module.  Make sure to click on each Argument in turn
///         and set an appropriate value such that you understand ahead of time what the module will do when it is run.
///     </para>
///     <para>
///         The data load engine design (RAW,STAGING,LIVE) makes it quite difficult to corrupt your data without realising
///         but you should still adopt best practice: Do as much data modification
///         in the RAW bubble (i.e. not as a post load operation), only use modules you understand the function of and try
///         to restrict the scope of your adjustment operations (it is usually better
///         to write an extraction transform than to transform the data during load in case there is a mistake or a
///         researcher wants uncorrupted original data).
///     </para>
/// </summary>
public partial class PluginProcessTaskUI : PluginProcessTaskUI_Design, ISaveableUI
{
    private ArgumentCollectionUI _argumentCollection;
    private Type _underlyingType;
    private ProcessTask _processTask;
    private readonly RAGSmileyToolStrip _ragSmiley;

    public PluginProcessTaskUI()
    {
        InitializeComponent();
        AssociatedCollection = RDMPCollection.DataLoad;

        _ragSmiley = new RAGSmileyToolStrip();
    }

    public override void SetDatabaseObject(IActivateItems activator, ProcessTask databaseObject)
    {
        _processTask = databaseObject;
        base.SetDatabaseObject(activator, databaseObject);

        if (_argumentCollection == null)
        {
            _argumentCollection = new ArgumentCollectionUI();

            var className = databaseObject.GetClassNameWhoArgumentsAreFor();

            if (string.IsNullOrWhiteSpace(className))
            {
                activator.KillForm(ParentForm, new Exception(
                    $"No class has been specified on ProcessTask '{databaseObject}'"));
                return;
            }

            try
            {
                _underlyingType = MEF.GetType(className);

                if (_underlyingType == null)
                    activator.KillForm(ParentForm, new Exception(
                        $"Could not find Type '{className}' for ProcessTask '{databaseObject}'"));
            }
            catch (Exception e)
            {
                activator.KillForm(ParentForm, new Exception(
                    $"MEF crashed while trying to look up Type '{className}' for ProcessTask '{databaseObject}'", e));
                return;
            }

            _argumentCollection.Setup(Activator, databaseObject, _underlyingType,
                Activator.RepositoryLocator.CatalogueRepository);

            _argumentCollection.Dock = DockStyle.Fill;
            pArguments.Controls.Add(_argumentCollection);
        }

        CommonFunctionality.Add(_ragSmiley);

        CheckComponent();

        loadStageIconUI1.Setup(Activator.CoreIconProvider, _processTask.LoadStage);

        CommonFunctionality.Add(new ToolStripButton("Check", FamFamFamIcons.arrow_refresh.ImageToBitmap(),
            (s, e) => CheckComponent()));
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, ProcessTask databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbName, "Text", "Name", d => d.Name);
        Bind(tbID, "Text", "ID", d => d.ID);
    }

    private void CheckComponent()
    {
        try
        {
            var lmd = _processTask.LoadMetadata;
            var argsDictionary = new LoadArgsDictionary(lmd, new HICDatabaseConfiguration(lmd).DeployInfo);
            var mefTask =
                (IMEFRuntimeTask)RuntimeTaskFactory.Create(_processTask,
                    argsDictionary.LoadArgs[_processTask.LoadStage]);

            _ragSmiley.StartChecking(mefTask.MEFPluginClassInstance);
        }
        catch (Exception e)
        {
            _ragSmiley.Fatal(e);
        }
    }

    private void tbName_TextChanged(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbName.Text))
        {
            tbName.Text = "No Name";
            tbName.SelectAll();
        }

        _processTask.Name = tbName.Text;
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<PluginProcessTaskUI_Design, UserControl>))]
public abstract class PluginProcessTaskUI_Design : RDMPSingleDatabaseObjectControl<ProcessTask>
{
}