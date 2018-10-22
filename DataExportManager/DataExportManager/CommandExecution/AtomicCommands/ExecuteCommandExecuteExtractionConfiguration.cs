using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportManager.ProjectUI;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExecuteExtractionConfiguration:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private ExtractionConfiguration _extractionConfiguration;
        private SelectedDataSets _selectedDataSet;

        [ImportingConstructor]
        public ExecuteCommandExecuteExtractionConfiguration(IActivateItems activator, ExtractionConfiguration extractionConfiguration) : this(activator)
        {
            _extractionConfiguration = extractionConfiguration;
        }

        public ExecuteCommandExecuteExtractionConfiguration(IActivateItems activator) : base(activator)
        {
            OverrideCommandName = "Extract...";
        }

        public ExecuteCommandExecuteExtractionConfiguration(IActivateItems activator, SelectedDataSets selectedDataSet) : this(activator)
        {
            _extractionConfiguration = (ExtractionConfiguration)selectedDataSet.ExtractionConfiguration;
            _selectedDataSet = selectedDataSet;

        }

        public override string GetCommandHelp()
        {
            return "Extract all the datasets in the configuration linking each against the configuration's cohort";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractionConfiguration,OverlayKind.Execute);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _extractionConfiguration = (ExtractionConfiguration)target;

            //must have datasets, must not be released and must have a cohort

            if(_extractionConfiguration.IsReleased)
            {
                SetImpossible("ExtractionConfiguration is released so cannot be executed");
                return this;
            }

            if(_extractionConfiguration.Cohort_ID == null)
            {
                SetImpossible("No cohort has been configured for ExtractionConfiguration");
                return this;
            }

            if (!_extractionConfiguration.GetAllExtractableDataSets().Any())
                SetImpossible("ExtractionConfiguration does not have an selected datasets");

            return this;
        }

        public override void Execute()
        {
            base.Execute();
            var ui = Activator.Activate<ExecuteExtractionUI, ExtractionConfiguration>(_extractionConfiguration);

            if (_selectedDataSet != null)
                ui.TickAllFor(_selectedDataSet);
        }
    }
}
