// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.DataQualityUIs;

/// <summary>
/// Form for performing Data Quality Engine executions on a chosen Catalogue. Opening the form will trigger a series of pre run checks and once these have successfully completed you
/// can then begin the execution by clicking the Start Execution button.
/// 
/// <para>While the execution is happening you can view the progress on the right hand side.</para>
/// 
/// <para>To view the results of the execution Right Click on the relevant catalogue and select View DQE Results.</para>
/// </summary>
public partial class DQEExecutionControlUI : DQEExecutionControl_Design
{
    private Catalogue _catalogue;

    public DQEExecutionControlUI()
    {
        InitializeComponent();

        AssociatedCollection = RDMPCollection.Catalogue;
        checkAndExecuteUI1.CommandGetter += CommandGetter;
        checkAndExecuteUI1.ExecutionFinished += checkAndExecuteUI1_ExecutionFinished;
    }

    private void checkAndExecuteUI1_ExecutionFinished(object sender, ExecutionEventArgs e)
    {
        //refresh
        SetDatabaseObject(Activator, _catalogue);
    }

    private RDMPCommandLineOptions CommandGetter(CommandLineActivity commandLineActivity) => new DqeOptions
    { Catalogue = _catalogue.ID.ToString(), Command = commandLineActivity };

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _catalogue = databaseObject;
        checkAndExecuteUI1.SetItemActivator(activator);

        CommonFunctionality.Add(new ExecuteCommandConfigureCatalogueValidationRules(Activator).SetTarget(_catalogue),
            "Validation Rules...");
        CommonFunctionality.Add(new ExecuteCommandViewDQEResultsForCatalogue(Activator)
        { OverrideCommandName = "View Results..." }.SetTarget(databaseObject));
    }

    public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
    {
        base.ConsultAboutClosing(sender, e);
        checkAndExecuteUI1.ConsultAboutClosing(sender, e);
    }

    public override string GetTabName() => $"DQE Execution:{base.GetTabName()}";
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DQEExecutionControl_Design, UserControl>))]
public abstract class DQEExecutionControl_Design : RDMPSingleDatabaseObjectControl<Catalogue>;