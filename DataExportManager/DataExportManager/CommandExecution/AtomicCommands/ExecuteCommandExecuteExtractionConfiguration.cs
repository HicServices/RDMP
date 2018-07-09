using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportManager.ProjectUI;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExecuteExtractionConfiguration:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private ExtractionConfiguration _extractionConfiguration;

        [ImportingConstructor]
        public ExecuteCommandExecuteExtractionConfiguration(IActivateItems activator, ExtractionConfiguration extractionConfiguration) : base(activator)
        {
            _extractionConfiguration = extractionConfiguration;
        }

        public ExecuteCommandExecuteExtractionConfiguration(IActivateItems activator) : base(activator)
        {
            
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.ExecuteArrow;
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
            Activator.Activate<ExecuteExtractionUI, ExtractionConfiguration>(_extractionConfiguration);
        }
    }
}
