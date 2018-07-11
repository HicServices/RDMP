using System.ComponentModel.Composition;
using System.Drawing;
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
    public class ExecuteCommandViewSelectedDataSetsExtractionSql:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private ExtractionConfiguration _extractionConfiguration;
        private SelectedDataSets _selectedDataSet;

        [ImportingConstructor]
        public ExecuteCommandViewSelectedDataSetsExtractionSql(IActivateItems activator,ExtractionConfiguration extractionConfiguration)
            : base(activator)
        {
            _extractionConfiguration = extractionConfiguration;
        }

        public ExecuteCommandViewSelectedDataSetsExtractionSql(IActivateItems activator) : base(activator)
        {
        }

        public override string GetCommandHelp()
        {
            return "Shows the SQL that will be executed for the given dataset when it is extracted including the linkage with the cohort table";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SQL,OverlayKind.Execute);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            if(target is SelectedDataSets)
            {
                _selectedDataSet =  target as SelectedDataSets;

                if (_selectedDataSet != null)
                    //must have datasets and have a cohort configured
                    if (_selectedDataSet.ExtractionConfiguration.Cohort_ID == null)
                        SetImpossible("No cohort has been selected for ExtractionConfiguration");
            }

            if(target is ExtractionConfiguration)
                _extractionConfiguration = target as ExtractionConfiguration;
            
            return this;
        }

        public override void Execute()
        {
            base.Execute();

            if (_selectedDataSet == null && _extractionConfiguration != null)
                _selectedDataSet = SelectOne(Activator.RepositoryLocator.DataExportRepository.GetAllObjectsWithParent<SelectedDataSets>(_extractionConfiguration));

            if(_selectedDataSet == null)
                return;

            Activator.Activate<ViewExtractionConfigurationSQLUI, SelectedDataSets>(_selectedDataSet);
        }
    }
}
