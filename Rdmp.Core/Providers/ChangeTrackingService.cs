using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
namespace Rdmp.Core.Providers
{
    // ChangeTrackingService.cs
    //
    // "What changed since version X" for a SQL Server-backed in-memory cache.
    //
    // Built on SQL Server's native Change Tracking feature rather than a hand-rolled
    // LastModified/timestamp column, because:
    //   - it tracks DELETEs as well as inserts/updates (a timestamp column can't)
    //   - it's versioned, so "give me everything since version 4821" is a first-class
    //     query (CHANGETABLE), not something you have to reconstruct
    //   - it's lightweight: SQL Server just tracks changed PKs + operation type, not
    //     full row history (that's SQL Server Temporal Tables / CDC, which are heavier
    //     tools for a different job — auditing/point-in-time queries, not cache sync)
    //
    // -----------------------------------------------------------------------------
    // ONE-TIME SQL SETUP (run once per database / per table you want to track):
    // -----------------------------------------------------------------------------
    //
    //   ALTER DATABASE [YourCatalogueDatabase]
    //   SET CHANGE_TRACKING = ON (CHANGE_RETENTION = 7 DAYS, AUTO_CLEANUP = ON);
    //
    //   ALTER TABLE dbo.Catalogue        ENABLE CHANGE_TRACKING;
    //   ALTER TABLE dbo.CatalogueItem    ENABLE CHANGE_TRACKING;
    //   ALTER TABLE dbo.ColumnInfo       ENABLE CHANGE_TRACKING;
    //   -- ...repeat for every table you want incremental sync on.
    //
    // CHANGE_RETENTION controls how far back history is kept — if a client asks for
    // changes since a version older than the retention window, SQL Server can no
    // longer answer incrementally and you must fall back to a full reload (handled
    // below via ChangeTrackingExpiredException).
    //
    // -----------------------------------------------------------------------------

    public enum ChangeOperation
    {
        Insert,
        Update,
        Delete
    }

    public readonly record struct ChangedRow(int Id, ChangeOperation Operation);

    public sealed record ChangeSet(
        long CurrentVersion,
        IReadOnlyDictionary<string, IReadOnlyList<ChangedRow>> ChangesByTable)
    {
        public bool IsEmpty => ChangesByTable.Count == 0;
    }

    /// <summary>
    /// Thrown when the requested version is older than SQL Server's Change Tracking
    /// retention window. There is no way to answer "what changed since X" at that
    /// point — the caller must do a full reload and start tracking from the new
    /// current version.
    /// </summary>
    public sealed class ChangeTrackingExpiredException : Exception
    {
        public long RequestedVersion { get; }
        public long MinValidVersion { get; }

        public ChangeTrackingExpiredException(long requestedVersion, long minValidVersion)
            : base($"Requested version {requestedVersion} is older than the minimum valid tracked " +
                   $"version {minValidVersion}. Change history has been cleaned up — a full reload is required.")
        {
            RequestedVersion = requestedVersion;
            MinValidVersion = minValidVersion;
        }
    }

    /// <summary>
    /// Queries SQL Server Change Tracking for rows that changed since a given version,
    /// across a fixed set of tracked tables. Intended to back an incremental refresh
    /// of an in-memory cache (e.g. "patch just these IDs" instead of "reload everything").
    /// </summary>
    public sealed class ChangeTrackingService
    {
        private readonly string _connectionString;
        private readonly IReadOnlyList<string> _trackedTables;

        public static readonly List<string> Catalogue_DEFAULT_TABLE_NAMES = new()
        {
            //Catalogue DB
            "Favourite",
            "Dataset",
            "Pipeline",
            "AggregateTopX",
            "PipelineComponent",
            "PipelineComponentArgument",
            "DashboardLayout",
            "DashboardControl",
            "DataAccessCredentials",
            "DashboardObjectUse",
            "RemoteRDMP",
            "ObjectImport",
            "ObjectExport",
            "CacheProgress",
            "ConnectionStringKeyword",
            "WindowLayout",
            "PermissionWindow",
            "TicketingSystemConfiguration",
            "CacheFetchFailure",
            "CohortAggregateContainer",
            "CohortIdentificationConfiguration",
            "ANOTable",
            "AggregateConfiguration",
            "GovernanceDocument",
            "AggregateContinuousDateAxis",
            "GovernancePeriod",
            "AggregateDimension",
            "AggregateFilter",
            "AggregateFilterContainer",
            "AggregateFilterParameter",
            "StandardRegex",
            "AnyTableSqlParameter",
            "Catalogue",
            "CatalogueItem",
            "CatalogueItemIssue",
            "Plugin",
            "ColumnInfo",
            "ExternalDatabaseServer",
            "ExtractionFilter",
            "ExtractionFilterParameter",
            "ExtractionInformation",
            "ExtendedProperty",
            "JoinInfo",
            "Commit",
            "LoadMetadata",
            "JoinableCohortAggregateConfiguration",
            "LoadModuleAssembly",
            "JoinableCohortAggregateConfigurationUse",
            "LoadProgress",
            "Lookup",
            "LookupCompositeJoinInfo",
            "PreLoadDiscardedColumn",
            "RegexRedactionConfiguration",
            "ProcessTask",
            "RegexRedaction",
            "ProcessTaskArgument",
            "ExtractionFilterParameterSet",
            "RegexRedactionKey",
            "SupportingDocument",
            "ExtractionFilterParameterSetValue",
            "SupportingSQLTable",
            "TableInfo"          
        };

        public static List<string> DataExport_DEFAULT_TABLE_NAMES = new()
        {
            //Data Export DB
            "ExtractableDataSetPackage",
            "ProjectCohortIdentificationConfigurationAssociation",
            "SelectedDataSetsForcedJoin",
            "SupplementalExtractionResults",
            "ExtractionProgress",
            "CumulativeExtractionResults",
            "DataUser",
            "DeployedExtractionFilter",
            "DeployedExtractionFilterParameter",
            "ExternalCohortTable",
            "ExtractableCohort",
            "ExtractableColumn",
            "ExtractableDataSet",
            "ExtractionConfiguration",
            "FilterContainer",
            "GlobalExtractionFilterParameter",
            "Project",
            "ReleaseLog",
            "SelectedDataSets"
        };


        public ChangeTrackingService(string connectionString, IReadOnlyList<string> trackedTables)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _trackedTables = trackedTables ?? throw new ArgumentNullException(nameof(trackedTables));
        }

        public long GetCurrentVersion()
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            return GetCurrentVersion(conn);
        }

        private static long GetCurrentVersion(SqlConnection conn)
        {
            using var cmd = new SqlCommand("SELECT CHANGE_TRACKING_CURRENT_VERSION()", conn);
            var result = cmd.ExecuteScalar();
            return result is null or DBNull ? 0 : Convert.ToInt64(result);
        }

        public ChangeSet GetChangesSince(long sinceVersion)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var currentVersion = GetCurrentVersion(conn);
            if (sinceVersion == currentVersion)
                return new ChangeSet(currentVersion, new Dictionary<string, IReadOnlyList<ChangedRow>>());

            var changesByTable = new Dictionary<string, IReadOnlyList<ChangedRow>>();

            foreach (var table in _trackedTables)
            {
                var minValid = GetMinValidVersion(conn, table);
                if (sinceVersion < minValid)
                    throw new ChangeTrackingExpiredException(sinceVersion, minValid);

                var rows = GetChangedRows(conn, table, sinceVersion);
                if (rows.Count > 0)
                    changesByTable[table] = rows;
            }

            return new ChangeSet(currentVersion, changesByTable);
        }

        private static long GetMinValidVersion(SqlConnection conn, string table)
        {
            using var cmd = new SqlCommand(
                "SELECT CHANGE_TRACKING_MIN_VALID_VERSION(OBJECT_ID(@tableName))", conn);
            cmd.Parameters.Add(new SqlParameter("@tableName", SqlDbType.NVarChar, 256) { Value = $"dbo.{table}" });

            var result = cmd.ExecuteScalar();
            // NULL means change tracking isn't enabled on this table at all.
            if (result is null or DBNull)
                //throw new InvalidOperationException(
                //    $"Table '{table}' does not have CHANGE_TRACKING enabled. " +
                //    "Run ALTER TABLE dbo.{table} ENABLE CHANGE_TRACKING; first.");
                return -1; // Return -1 to indicate that change tracking is not enabled for this table.

            return Convert.ToInt64(result);
        }

        private static List<ChangedRow> GetChangedRows(
            SqlConnection conn, string table, long sinceVersion)
        {
            // Table name is validated against the caller-supplied _trackedTables list (not
            // user input), so string interpolation here is safe — CHANGETABLE's target table
            // can't be parameterised in T-SQL.
            var sql = $"""
            SELECT ct.ID, ct.SYS_CHANGE_OPERATION
            FROM CHANGETABLE(CHANGES dbo.[{table}], @sinceVersion) AS ct
            ORDER BY ct.SYS_CHANGE_VERSION
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@sinceVersion", SqlDbType.BigInt) { Value = sinceVersion });

            var rows = new List<ChangedRow>();
            Console.WriteLine("changing: " + table);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var operation = reader.GetString(1) switch
                {
                    "I" => ChangeOperation.Insert,
                    "U" => ChangeOperation.Update,
                    "D" => ChangeOperation.Delete,
                    var other => throw new InvalidOperationException(
                        $"Unexpected SYS_CHANGE_OPERATION '{other}' for table '{table}'.")
                };
                rows.Add(new ChangedRow(id, operation));
            }

            return rows;
        }
    }
}
