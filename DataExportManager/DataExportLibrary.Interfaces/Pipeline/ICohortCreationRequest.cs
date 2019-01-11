using System.Data.SqlClient;
using CatalogueLibrary.Data.Pipelines;
using DataExportLibrary.Interfaces.Data.DataTables;
using FAnsi.Connections;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Interfaces.Pipeline
{
    /// <summary>
    /// See CohortCreationRequest
    ///  </summary>
    public interface ICohortCreationRequest : ICheckable, IHasDesignTimeMode
    {
        IProject Project { get; }
        ICohortDefinition NewCohortDefinition { get; set; }
        
        int ImportAsExtractableCohort(bool deprecateOldCohortOnSuccess);
        void PushToServer(IManagedConnection transaction);
    }
}