using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.Tutorials;
using CohortManager.CommandExecution.AtomicCommands;
using DataExportManager.Collections.Providers;
using DataExportManager.CommandExecution.AtomicCommands;
using MapsDirectlyToDatabaseTable;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.HomePane
{
    /// <summary>
    /// The starting page of RDMP.  Provides a single easy access entry point into RDMP functionality for common tasks e.g. Data Management, Project Extraction etc.  Click the links of commands
    /// you want to carry out to access wizards that offer streamlined access to the RDMP functionality.
    /// 
    /// You can access the HomeUI at any time by clicking the home icon in the top left of the RDMP tool bar.
    /// </summary>
    public partial class HomeUI : UserControl,ILifetimeSubscriber
    {
        private readonly ToolboxWindowManager windowManager;
        private AtomicCommandUIFactory _uiFactory;

        public HomeUI(ToolboxWindowManager windowManager)
        {
            this.windowManager = windowManager;
            _uiFactory = new AtomicCommandUIFactory(windowManager.ContentManager.CoreIconProvider);
            InitializeComponent();
        }

        private void BuildCommandLists()
        {
            tlpDataManagement.Controls.Clear();
            tlpCohortCreation.Controls.Clear();
            tlpDataExport.Controls.Clear();
            tlpDataLoad.Controls.Clear();
            
            /////////////////////////////////////Data Management/////////////////////////////////
            AddCommand(new ExecuteCommandCreateNewCatalogueByImportingFile(windowManager.ContentManager),tlpDataManagement);
            
            AddCommand(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(windowManager.ContentManager, true),tlpDataManagement);

            AddCommand(new ExecuteCommandEditExistingCatalogue(windowManager.ContentManager),
                windowManager.ContentManager.CoreChildProvider.AllCatalogues,
                cata => cata.Name,
                tlpDataManagement);

            /////////////////////////////////////Cohort Creation/////////////////////////////////

            AddCommand(new ExecuteCommandCreateNewCohortIdentificationConfiguration(windowManager.ContentManager),tlpCohortCreation);

            AddCommand(new ExecuteCommandEditExistingCohortIdentificationConfiguration(windowManager.ContentManager),
                    windowManager.ContentManager.CoreChildProvider.AllCohortIdentificationConfigurations,
                    cic => cic.Name,
                    tlpCohortCreation);

            /////////////////////////////////////Data Export/////////////////////////////////
            
            var dataExportChildProvider = windowManager.ContentManager.CoreChildProvider as DataExportChildProvider;
            if (dataExportChildProvider != null)
            {
                AddCommand(new ExecuteCommandCreateNewDataExtractionProject(windowManager.ContentManager), tlpDataExport);
                AddCommand(new ExecuteCommandEditAndRunExistingDataExtractionProject(windowManager.ContentManager),
                        dataExportChildProvider.Projects,
                        cic => cic.Name,
                        tlpDataExport);

                AddCommand(new ExecuteCommandImportFileAsCustomDataForCohort(windowManager.ContentManager), 
                        dataExportChildProvider.Cohorts,
                        c => c.ToString(),
                        tlpDataExport);
            }

            //////////////////////////////////Data Loading////////////////////////////////////
            AddCommand(new ExecuteCommandCreateNewLoadMetadata(windowManager.ContentManager),tlpDataLoad);
            AddCommand(new ExecuteCommandEditExistingLoadMetadata(windowManager.ContentManager), 
                windowManager.ContentManager.CoreChildProvider.AllLoadMetadatas,
                lmd=>lmd.Name,
                tlpDataLoad);
            
            FixSizingOfTableLayoutPanel(tlpDataManagement);
            FixSizingOfTableLayoutPanel(tlpCohortCreation);
            FixSizingOfTableLayoutPanel(tlpDataExport);
            FixSizingOfTableLayoutPanel(tlpDataLoad);
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

            if (windowManager == null)
                return;

            BuildCommandLists();

            windowManager.ContentManager.RefreshBus.EstablishLifetimeSubscription(this);
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            BuildCommandLists();
        }
    }

    
}
