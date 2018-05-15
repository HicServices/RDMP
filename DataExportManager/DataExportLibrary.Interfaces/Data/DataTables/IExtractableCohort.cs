using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See ExtractableCohort
    /// </summary>
    public interface IExtractableCohort : IMapsDirectlyToDatabaseTable, ISaveable, IHasQuerySyntaxHelper
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