using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.DataRelease
{
    public class NullReleasePotential : ReleasePotential
    {
        public NullReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ExtractionConfiguration configuration, ExtractableDataSet dataSet) : base(repositoryLocator, configuration, dataSet)
        {
        }

        protected override Releaseability GetSpecificAssessment()
        {
            return Releaseability.NeverBeenSuccessfullyExecuted;
        }
    }
}