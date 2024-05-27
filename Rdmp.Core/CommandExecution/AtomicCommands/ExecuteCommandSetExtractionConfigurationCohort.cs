using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandSetExtractionConfigurationCohort : BasicCommandExecution
    {
        private ExtractionConfiguration _extractionConfiguration;
        private ExtractableCohort _extractableCohort;

        public ExecuteCommandSetExtractionConfigurationCohort(IBasicActivateItems activator) : base(activator)
        {
            Weight = 100.1f;
        }

        public ExecuteCommandSetExtractionConfigurationCohort(IBasicActivateItems activator, ExtractionConfiguration extractionConfiguration, ExtractableCohort cohort) : this(activator)
        {
            _extractionConfiguration = extractionConfiguration;
            _extractableCohort = cohort;
        }


        public override void Execute()
        {
            base.Execute();
            _extractionConfiguration.Cohort_ID = _extractableCohort.ID;
            _extractionConfiguration.SaveToDatabase();
        }

    }
}
