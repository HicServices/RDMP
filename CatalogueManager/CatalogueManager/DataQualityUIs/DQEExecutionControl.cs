using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using RDMPAutomationService.Options;
using RDMPAutomationService.Options.Abstracts;
using ReusableLibraryCode.Icons.IconProvision;
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
        private bool _firstTime=true;

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
            
            if(_firstTime)
            {
                Add(toolStrip1, new ExecuteCommandConfigureCatalogueValidationRules(_activator).SetTarget(_catalogue));
                AddPluginCommands(toolStrip1, this,_catalogue);
            }
            _firstTime = false;
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
