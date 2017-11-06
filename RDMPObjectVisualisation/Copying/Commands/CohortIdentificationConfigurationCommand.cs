using CatalogueLibrary.Data.Cohort;
using ReusableUIComponents.CommandExecution;

namespace RDMPObjectVisualisation.Copying.Commands
{
    public class CohortIdentificationConfigurationCommand:ICommand
    {
        public CohortIdentificationConfiguration CohortIdentificationConfiguration { get; set; }

        public CohortIdentificationConfigurationCommand(CohortIdentificationConfiguration cohortIdentificationConfiguration)
        {
            CohortIdentificationConfiguration = cohortIdentificationConfiguration;
        }

        public string GetSqlString()
        {
            return null;
        }
    }
}
