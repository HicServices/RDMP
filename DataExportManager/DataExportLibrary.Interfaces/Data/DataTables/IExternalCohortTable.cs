using FAnsi.Connections;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See ExternalCohortTable
    /// </summary>
    public interface IExternalCohortTable : ICheckable, IDataAccessPoint, IHasDependencies,INamed
    {
        /// <summary>
        /// Name of the table in your database that contains the private to release identifier mappings (e.g. 'Cohort').
        /// </summary>
        string TableName { get; set; }

        /// <summary>
        /// Name of the inventory table in your database which records the names and versions of cohorts stored in the cohort table (e.g. 'CohortDefinition').
        /// </summary>
        string DefinitionTableName { get; set; }

        /// <summary>
        /// The column in the cohort table which contains private identifiers (e.g. 'chi').  This column must contain identifiers in the same format 
        /// as the datasets you want to link with when performing project extractions.
        /// </summary>
        string PrivateIdentifierField { get; set; }

        /// <summary>
        /// The column in the cohort table which contains release identifiers (e.g. 'ReleaseId).  These are the values that will be substituted in during
        /// the linkage stage of project extractions.
        /// </summary>
        string ReleaseIdentifierField { get; set; }

        /// <summary>
        /// The name of the field in the cohort (mapping) table which is a foreign key into <see cref="DefinitionTableName"/> (cohort descriptions) table (e.g. cohortDefinition_id).
        /// </summary>
        string DefinitionTableForeignKeyField { get; set; }
        
        DiscoveredDatabase Discover();
        
        void PushToServer(ICohortDefinition newCohortDefinition, IManagedConnection connection);
        bool IDExistsInCohortTable(int originID);
        string GetReleaseIdentifier(IExtractableCohort cohort);
        IExternalCohortDefinitionData GetExternalData(IExtractableCohort extractableCohort);
    }
}