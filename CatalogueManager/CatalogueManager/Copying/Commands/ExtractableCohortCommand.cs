using System;
using System.Linq;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.Copying.Commands
{
    public class ExtractableCohortCommand : ICommand
    {
        public int ExternalProjectNumber { get; set; }
        public ExtractableCohort Cohort { get; set; }

        public Exception ErrorGettingCohortData { get; private set; }
        
        public IExtractionConfiguration[] CompatibleExtractionConfigurations { get; set; }
        public Project[] CompatibleProjects { get; set; }

        public ExtractableCohortCommand(ExtractableCohort extractableCohort)
        {
            Cohort = extractableCohort;

            try
            {
                ExternalProjectNumber = Cohort.GetExternalData().ExternalProjectNumber;
            }
            catch (Exception e)
            {
                ErrorGettingCohortData = e;
                return;
            }

            CompatibleProjects = extractableCohort.Repository.GetAllObjects<Project>("WHERE ProjectNumber = " + ExternalProjectNumber);
            CompatibleExtractionConfigurations = CompatibleProjects.SelectMany(p => p.ExtractionConfigurations).ToArray();
        }
        
        public string GetSqlString()
        {
            return Cohort.WhereSQL();
        }
    }
}