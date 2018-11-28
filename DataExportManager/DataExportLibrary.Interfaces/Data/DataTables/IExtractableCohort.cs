using System.Data;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See ExtractableCohort
    /// </summary>
    public interface IExtractableCohort :  IHasQuerySyntaxHelper, IMightBeDeprecated
    {
        int CountDistinct { get; }
        int ExternalCohortTable_ID { get; }
        int OriginID { get; }
        string OverrideReleaseIdentifierSQL { get; set; }
        IExternalCohortTable ExternalCohortTable { get; }

        DataTable FetchEntireCohort();
        string GetPrivateIdentifier(bool runtimeName = false);
        string GetReleaseIdentifier(bool runtimeName = false);
        DataTable GetReleaseIdentifierMap(IDataLoadEventListener listener);
        string WhereSQL();
        string GetFirstProCHIPrefix();
        IExternalCohortDefinitionData GetExternalData();
        string GetPrivateIdentifierDataType();
        string GetReleaseIdentifierDataType();

        DiscoveredDatabase GetDatabaseServer();
        void ReverseAnonymiseDataTable(DataTable toProcess, IDataLoadEventListener listener, bool allowCaching);
        
    }
}