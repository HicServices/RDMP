// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using RDMPAutomationService.Options;
using RDMPAutomationService.Options.Abstracts;
using ReusableUIComponents;

namespace CatalogueManager.DataQualityUIs
{
    /// <summary>
    /// Form for performing Data Quality Engine executions on a chosen Catalogue. Opening the form will trigger a series of pre run checks and once these have successfully completed you
    /// can then begin the execution by clicking the Start Execution button.
    /// 
    /// <para>While the execution is happening you can view the progress on the right hand side.</para>
    /// 
    /// <para>To view the results of the execution Right Click on the relevant catalogue and select View DQE Results.</para>
    /// </summary>
    public partial class DQEExecutionControl : DQEExecutionControl_Design
    {
        private Catalogue _catalogue;
        
        public DQEExecutionControl()
        {
            InitializeComponent();
            
            AssociatedCollection = RDMPCollection.Catalogue;
            checkAndExecuteUI1.CommandGetter += CommandGetter;
        }

        private RDMPCommandLineOptions CommandGetter(CommandLineActivity commandLineActivity)
        {
            return new DqeOptions() { Catalogue = _catalogue.ID, Command = commandLineActivity };
        }

        public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            _catalogue = databaseObject;
            checkAndExecuteUI1.SetItemActivator(activator);
            
            Add(new ExecuteCommandConfigureCatalogueValidationRules(_activator).SetTarget(_catalogue),"Validation Rules...");
            AddPluginCommands();
        }
        
        public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            base.ConsultAboutClosing(sender,e);
            checkAndExecuteUI1.ConsultAboutClosing(sender,e);
        }

        public override string GetTabName()
        {
            return "DQE Execution:" + base.GetTabName();
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DQEExecutionControl_Design, UserControl>))]
    public abstract class DQEExecutionControl_Design : RDMPSingleDatabaseObjectControl<Catalogue>
    {
    }
}
