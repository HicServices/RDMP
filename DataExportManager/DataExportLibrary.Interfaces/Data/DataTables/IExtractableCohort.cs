using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See ExtractableCohort
    /// </summary>
    public interface IExtractableCohort : IMapsDirectlyToDatabaseTable, ISaveable
    {
        int CountDistinct { get; }
        int ExternalCohortTable_ID { get; }
        int OriginID { get; }
        string OverrideReleaseIdentifierSQL { get; set; }
        IColumn[] CustomCohortColumns { get; }
        IExternalCohortTable ExternalCohortTable { get; }

        DataTable FetchEntireCohort();
        IEnumerable<string> GetCustomTableJoinSQLIfExists(QueryBuilder queryBuilder);
        string GetPrivateIdentifier();
        string GetReleaseIdentifier();
        DataTable GetReleaseIdentifierMap(IDataLoadEventListener listener);
        string WhereSQL();
        int CreateCustomColumnsIfCustomTableExists();
        string GetFirstProCHIPrefix();
        IExternalCohortDefinitionData GetExternalData();
        void DeleteCustomData(string tableName);
        string GetPrivateIdentifierDataType();
        void RecordNewCustomTable(DiscoveredServer server, string tableName, DbConnection con, DbTransaction transaction);
        IEnumerable<string> GetCustomTableNames();
        IEnumerable<string> GetCustomTableExtractionSQLs();
        string GetCustomTableExtractionSQL(string customTable,bool top100 =false);
        DiscoveredDatabase GetDatabaseServer();
        void ReverseAnonymiseDataTable(DataTable toProcess, IDataLoadEventListener listener, bool allowCaching);
    }
}