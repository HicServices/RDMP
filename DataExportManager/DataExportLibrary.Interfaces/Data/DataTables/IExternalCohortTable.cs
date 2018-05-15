using System.Data.Common;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See ExternalCohortTable
    /// </summary>
    public interface IExternalCohortTable : ICheckable, IDataAccessPoint,IMapsDirectlyToDatabaseTable
    {
        string Name { get; set; }
        string TableName { get; set; }
        string DefinitionTableName { get; set; }
        string PrivateIdentifierField { get; set; }
        string ReleaseIdentifierField { get; set; }
        string DefinitionTableForeignKeyField { get; set; }
        
        DiscoveredDatabase GetExpectDatabase();
        void PushToServer(ICohortDefinition newCohortDefinition, DbConnection con, DbTransaction transaction);
        bool IDExistsInCohortTable(int originID);
        string GetReleaseIdentifier(IExtractableCohort cohort);
    }
}