using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;

namespace CatalogueManager.AggregationUIs.Advanced.Options
{
    public class AggregateEditorOptionsFactory
    {
        public IAggregateEditorOptions Create(AggregateConfiguration config)
        {
            var cohortIdentificationConfiguration = config.GetCohortIdentificationConfigurationIfAny();

            if (cohortIdentificationConfiguration != null)
                return new AggregateEditorCohortOptions(cohortIdentificationConfiguration.GetAllParameters());

            return new AggregateEditorBasicOptions();
        }
    }
}
