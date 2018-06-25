using System.Data.SqlClient;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataExportLibrary.Interfaces.Pipeline
{
    /// <summary>
    /// See CohortCreationRequest
    ///  </summary>
    public interface ICohortCreationRequest:ICheckable
    {
        IProject Project { get; }
        ICohortDefinition NewCohortDefinition { get; set; }
        
        int ImportAsExtractableCohort();
        void PushToServer(IManagedConnection transaction);
    }
}