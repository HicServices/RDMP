using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.CommandExecution.AtomicCommands.WindowArranging;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Refreshing;
using CatalogueManager.Tutorials;
using CohortManager.CommandExecution.AtomicCommands;
using DataExportLibrary.Providers;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.HomePane
{
    /// <summary>
    /// The starting page of RDMP.  Provides a single easy access entry point into RDMP functionality for common tasks e.g. Data Management, Project Extraction etc.  Click the links of commands
    /// you want to carry out to access wizards that offer streamlined access to the RDMP functionality.
    /// 
    /// <para>You can access the HomeUI at any time by clicking the home icon in the top left of the RDMP tool bar.</para>
    /// </summary>
    public partial class HomeUI : UserControl,ILifetimeSubscriber
    {
        private readonly WindowManager _windowManager;
        private readonly AtomicCommandUIFactory _uiFactory;

        public HomeUI(WindowManager windowManager)
        {
            this._windowManager = windowManager;
            _uiFactory = new AtomicCommandUIFactory(windowManager.ActivateItems);
            InitializeComponent();
        }

        private void BuildCommandLists()
        {
            tlpDataManagement.Controls.Clear();
            tlpCohortCreation.Controls.Clear();
            tlpDataExport.Controls.Clear();
            tlpDataLoad.Controls.Clear();

            var activator = _windowManager.ActivateItems;
            
            /////////////////////////////////////Data Management/////////////////////////////////
            AddCommand(new ExecuteCommandCreateNewCatalogueByImportingFile(activator),tlpDataManagement);
            
            AddCommand(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(activator, true),tlpDataManagement);

            AddCommand(new ExecuteCommandEditExistingCatalogue(activator),
                activator.CoreChildProvider.AllCatalogues,
                cata => cata.Name,
                tlpDataManagement);

            AddCommand(
                new ExecuteCommandRunDQEOnCatalogue(activator),
                activator.CoreChildProvider.AllCatalogues, cata => cata.Name,
                tlpDataManagement);

            /////////////////////////////////////Cohort Creation/////////////////////////////////

            AddCommand(new ExecuteCommandCreateNewCohortFromFile(activator),
                tlpCohortCreation);

            AddCommand(new ExecuteCommandCreateNewCohortIdentificationConfiguration(activator),tlpCohortCreation);

            AddCommand(new ExecuteCommandEditExistingCohortIdentificationConfiguration(activator),
                    activator.CoreChildProvider.AllCohortIdentificationConfigurations,
                    cic => cic.Name,
                    tlpCohortCreation);
            
            AddCommand(new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(activator), 
                    activator.CoreChildProvider.AllCohortIdentificationConfigurations,
                    cic => cic.Name,
                    tlpCohortCreation);

            AddCommand(new ExecuteCommandCreateNewCohortFromCatalogue(activator),
                activator.CoreChildProvider.AllCatalogues,
                c=>c.Name,
tlpCohortCreation);
            
            /////////////////////////////////////Data Export/////////////////////////////////
            
            var dataExportChildProvider = activator.CoreChildProvider as DataExportChildProvider;
            if (dataExportChildProvider != null)
            {
                AddCommand(new ExecuteCommandCreateNewDataExtractionProject(activator), tlpDataExport);
                AddCommand(new ExecuteCommandEditAndRunExistingDataExtractionProject(activator),
                        dataExportChildProvider.Projects,
                        cic => cic.Name,
                        tlpDataExport);

                AddCommand(new ExecuteCommandMakeCatalogueProjectSpecific(activator),
                    dataExportChildProvider.AllCatalogues.Where(c=>!c.IsProjectSpecific(null)).ToArray(),
                    c=>c.Name,tlpDataExport );
            }

            //////////////////////////////////Data Loading////////////////////////////////////
            AddCommand(new ExecuteCommandCreateNewLoadMetadata(activator),tlpDataLoad);
            AddCommand(new ExecuteCommandExecuteLoadMetadata(activator), 
                activator.CoreChildProvider.AllLoadMetadatas,
                lmd=>lmd.Name,
                tlpDataLoad);
            
            FixSizingOfTableLayoutPanel(tlpDataManagement);
            FixSizingOfTableLayoutPanel(tlpCohortCreation);
            FixSizingOfTableLayoutPanel(tlpDataExport);
            FixSizingOfTableLayoutPanel(tlpDataLoad);


            //////////////////////////////////Advanced////////////////////////////////////
            AddCommand(new ExecuteCommandManagePlugins(activator),tlpAdvanced);
        }

        private void AddCommand<T>(IAtomicCommandWithTarget command, IEnumerable<T> selection, Func<T, string> propertySelector, TableLayoutPanel tableLayoutPanel)
        {
            var control = _uiFactory.CreateLinkLabelWithSelection(command, selection, propertySelector);
            tableLayoutPanel.Controls.Add(control, 0, tableLayoutPanel.Controls.Count);

            //extend the size to match
            var panel = (Panel)tableLayoutPanel.Parent;
            panel.Width = Math.Max(panel.Width, control.Width + 10);
        }

        private void AddCommand(IAtomicCommand command, TableLayoutPanel tableLayoutPanel)
        {
            var control = _uiFactory.CreateLinkLabel(command);
            tableLayoutPanel.Controls.Add(control, 0, tableLayoutPanel.Controls.Count);

            //extend the size to match
            var panel = (Panel)tableLayoutPanel.Parent;
            panel.Width = Math.Max(panel.Width, control.Width + 10);
        }

        private void FixSizingOfTableLayoutPanel(TableLayoutPanel tableLayoutPanel)
        {
            foreach (RowStyle style in tableLayoutPanel.RowStyles)
                style.SizeType = SizeType.AutoSize;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_windowManager == null)
                return;

            BuildCommandLists();

            _windowManager.ActivateItems.RefreshBus.EstablishLifetimeSubscription(this);
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            BuildCommandLists();
        }
    }
}
