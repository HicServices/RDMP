using CatalogueLibrary.Data.Cohort;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.Copying.Commands
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
