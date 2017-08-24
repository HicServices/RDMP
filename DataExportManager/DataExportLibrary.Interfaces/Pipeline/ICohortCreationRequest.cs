using System.Data.SqlClient;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Interfaces.Pipeline
{
    public interface ICohortCreationRequest:ICheckable
    {
        IProject Project { get; }
        ICohortDefinition NewCohortDefinition { get; set; }
        
        int ImportAsExtractableCohort();
        void PushToServer(SqlConnection con, SqlTransaction transaction);
    }
}