using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.CohortCreationPipeline.Sources;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableLibraryCode.Progress;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration : BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private AggregateConfiguration _aggregateConfiguration;
        private ExtractableCohort _cohort;

        public ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration(IActivateItems activator) : base(activator)
        {
            
        }

        public override void Execute()
        {
            base.Execute();

            if (_aggregateConfiguration == null)
                _aggregateConfiguration = SelectOne<AggregateConfiguration>(Activator.RepositoryLocator.CatalogueRepository);

            if(_aggregateConfiguration == null)
                return;

            IDataFlowSource<DataTable> fixedSource = null;

            if (_aggregateConfiguration.IsJoinablePatientIndexTable())
            {
                var dr = MessageBox.Show("Would you like to constrain the records to only those in a committed cohort?","Cohort Records Only", MessageBoxButtons.YesNoCancel);

                if(dr == DialogResult.Cancel)
                    return;

                if (dr == DialogResult.Yes)
                {
                    _cohort = SelectOne<ExtractableCohort>(Activator.RepositoryLocator.DataExportRepository);
                    
                    if(_cohort == null)
                        return;
                }

                if(_cohort != null)
                {
                    var src = new PatientIndexTableSource();
                    src.PreInitialize(_aggregateConfiguration, new ThrowImmediatelyDataLoadEventListener());
                    src.PreInitialize(_cohort, new ThrowImmediatelyDataLoadEventListener());
                    fixedSource = src;
                }
            }

            if(fixedSource == null)
            {
                var src = new AggregateConfigurationTableSource();
                src.PreInitialize(_aggregateConfiguration,new ThrowImmediatelyDataLoadEventListener());
            }


            //todo execute and commit!
        }


        public Image GetImage(IIconProvider iconProvider)
        {
            return _aggregateConfiguration == null
                ? iconProvider.GetImage(RDMPConcept.AggregateGraph)
                : iconProvider.GetImage(_aggregateConfiguration);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            var configuration = target as AggregateConfiguration;
            if (configuration != null)
                _aggregateConfiguration = configuration;

            var cohort = target as ExtractableCohort;
            if (cohort != null)
                _cohort = cohort;

            return this;
        }
    }
}