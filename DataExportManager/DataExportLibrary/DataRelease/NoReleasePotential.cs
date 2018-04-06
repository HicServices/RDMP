using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.DataRelease
{
    /// <summary>
    /// Release Potential class to be used when nothing has ever been extracted
    /// </summary>
    public class NoReleasePotential : ReleasePotential
    {
        public NoReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IExtractionConfiguration configuration, IExtractableDataSet dataSet) : base(repositoryLocator, configuration, dataSet)
        {
        }

        protected override Releaseability GetSpecificAssessment()
        {
            return Releaseability.NeverBeenSuccessfullyExecuted;
        }
    }
}