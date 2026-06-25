// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.IO;
using System.Linq;
using Rdmp.Core.CohortCreation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Shared logic for breaking a cohort's final inclusion list down by Scottish health board: joins
/// the cohort's patient identifiers to a demography catalogue's region column and writes a
/// per-board / per-node <c>COUNT(DISTINCT chi)</c> to CSV (see <see cref="HealthBoardBreakdownReport"/>
/// and <see cref="HealthBoardLookup"/>). Concrete subclasses supply the cohort identifier list from
/// either a committed <see cref="ExtractableCohort"/> or a live
/// <see cref="CohortIdentificationConfiguration"/>.
/// </summary>
public abstract class ExecuteCommandExportHealthBoardBreakdownBase : BasicCommandExecution
{
    protected readonly string DemographyCatalogue;
    protected readonly string RegionColumn;
    protected readonly int Timeout;
    protected FileInfo ToFile;

    private ExtractionInformation _regionEi;
    private ExtractionInformation _idEi;

    protected ExecuteCommandExportHealthBoardBreakdownBase(IBasicActivateItems activator,
        FileInfo toFile, string demographyCatalogue, string regionColumn, int timeout) : base(activator)
    {
        DemographyCatalogue = demographyCatalogue;
        RegionColumn = regionColumn;
        Timeout = timeout;
        ToFile = toFile;
    }

    /// <summary>A short name for the cohort source, used for the default output file name.</summary>
    protected abstract string SourceName { get; }

    /// <summary>
    /// Produces the cohort identifier sub-query, any parameter DECLARE SQL that must be hoisted to
    /// the front of the batch, and the cohort's own distinct patient count (for reconciliation).
    /// </summary>
    protected abstract (string subquery, string paramSql, int distinctCount) GetCohortIdentifierSql();

    /// <summary>
    /// Resolves the demography catalogue's region + identifier columns and marks the command
    /// impossible if anything is missing. Call from each subclass constructor once its cohort input
    /// has been validated.
    /// </summary>
    protected void ResolveDemography()
    {
        var demography = BasicActivator.RepositoryLocator.CatalogueRepository
            .GetAllObjects<Catalogue>()
            .FirstOrDefault(c => string.Equals(c.Name, DemographyCatalogue, System.StringComparison.OrdinalIgnoreCase));

        if (demography == null)
        {
            SetImpossible($"Could not find a catalogue called '{DemographyCatalogue}'");
            return;
        }

        var eis = demography.GetAllExtractionInformation(ExtractionCategory.Any);
        _regionEi = eis.FirstOrDefault(e =>
            string.Equals(e.GetRuntimeName(), RegionColumn, System.StringComparison.OrdinalIgnoreCase));
        _idEi = eis.FirstOrDefault(e => e.IsExtractionIdentifier);

        if (_regionEi == null)
            SetImpossible($"'{DemographyCatalogue}' has no column called '{RegionColumn}'");
        else if (_idEi == null)
            SetImpossible($"'{DemographyCatalogue}' has no IsExtractionIdentifier column to join the cohort on");
    }

    public override void Execute()
    {
        base.Execute();

        ToFile ??= BasicActivator.IsInteractive
            ? BasicActivator.SelectFile("Path to write health board breakdown to", "Health board breakdown", "*.csv")
            : new FileInfo(Path.Combine(System.Environment.CurrentDirectory, $"{Sanitise(SourceName)}-healthboard.csv"));

        if (ToFile == null)
            return;

        // The cohort identifier list differs by input type; everything downstream is identical.
        var (cohortIdSubquery, paramSql, cohortDistinct) = GetCohortIdentifierSql();

        var sql = paramSql + BuildBreakdownSql(
            regionSelect: _regionEi.SelectSQL ?? _regionEi.ColumnInfo.Name,
            chiSelect: _idEi.SelectSQL ?? _idEi.ColumnInfo.Name,
            demographyTable: _idEi.ColumnInfo.TableInfo.Name,
            cohortIdSubquery: cohortIdSubquery);

        // Run on the demography server; cohort tables are on the same server (3-part names resolve).
        var db = DataAccessPortal.ExpectDatabase(_idEi.ColumnInfo.TableInfo, DataAccessContext.InternalDataProcessing);

        var dt = new DataTable();
        using (var con = db.Server.GetConnection())
        {
            con.Open();
            using var cmd = db.Server.GetCommand(sql, con);
            cmd.CommandTimeout = Timeout;
            using var da = db.Server.GetDataAdapter(cmd);
            da.Fill(dt);
        }

        var records = HealthBoardBreakdownReport.BuildRecords(dt, "Region", "n");
        File.WriteAllText(ToFile.FullName, HealthBoardBreakdownReport.ToCsv(records));

        var inBreakdown = records
            .Where(r => r.Kind == HealthBoardBreakdownReport.RowKind.GrandTotal)
            .Select(r => r.Count)
            .FirstOrDefault();

        // Reconcile against the cohort's own distinct count: any shortfall = patients absent from
        // the demography catalogue (those never reach the GROUP BY join).
        var summary = $"Exported health board breakdown to {ToFile.FullName} ({inBreakdown} of {cohortDistinct} patients matched in '{DemographyCatalogue}'";
        var missing = cohortDistinct - inBreakdown;
        summary += missing > 0
            ? $"; {missing} not found in the demography catalogue)"
            : ")";

        BasicActivator.Show(summary);
    }

    /// <summary>Runs a count of the cohort identifier list (with hoisted parameters).</summary>
    protected int CountCohort(string paramSql, string body)
    {
        var db = DataAccessPortal.ExpectDatabase(_idEi.ColumnInfo.TableInfo, DataAccessContext.InternalDataProcessing);
        using var con = db.Server.GetConnection();
        con.Open();
        using var cmd = db.Server.GetCommand($"{paramSql}SELECT count(*) FROM ({body}) _hb_recon", con);
        cmd.CommandTimeout = Timeout;
        return System.Convert.ToInt32(cmd.ExecuteScalar());
    }

    /// <summary>
    /// Composes the per-region count query (pure string; unit-testable without a database).
    /// </summary>
    public static string BuildBreakdownSql(string regionSelect, string chiSelect, string demographyTable,
        string cohortIdSubquery) =>
        $"""
         SELECT {regionSelect} AS Region,
                COUNT(DISTINCT {chiSelect}) AS n
         FROM {demographyTable}
         WHERE {chiSelect} IN (
             {cohortIdSubquery}
         )
         GROUP BY {regionSelect}
         """;

    protected static string Sanitise(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}

/// <summary>
/// Breaks a committed <see cref="ExtractableCohort"/>'s released inclusion list down by health board.
/// </summary>
public class ExecuteCommandExportCohortHealthBoardBreakdown : ExecuteCommandExportHealthBoardBreakdownBase
{
    private readonly ExtractableCohort _cohort;

    public ExecuteCommandExportCohortHealthBoardBreakdown(IBasicActivateItems activator,
        [DemandsInitialization("The committed cohort whose final inclusion list to break down")]
        ExtractableCohort cohort,
        [DemandsInitialization("CSV file to write. Defaults to <cohort>-healthboard.csv in the current directory")]
        FileInfo toFile = null,
        [DemandsInitialization("Name of the demography catalogue holding the region column", DefaultValue = "SHARE_Demography")]
        string demographyCatalogue = "SHARE_Demography",
        [DemandsInitialization("Name of the region (health board cipher) column on the demography catalogue", DefaultValue = "Region")]
        string regionColumn = "Region",
        [DemandsInitialization("Per-query command timeout in seconds", DefaultValue = 5000)]
        int timeout = 5000) : base(activator, toFile, demographyCatalogue, regionColumn, timeout)
    {
        _cohort = cohort;

        if (_cohort == null)
            SetImpossible("No cohort was supplied");
        else
            ResolveDemography();
    }

    protected override string SourceName => _cohort.ToString();

    protected override (string subquery, string paramSql, int distinctCount) GetCohortIdentifierSql()
    {
        // Committed cohort: a flat table. Fully qualify it (usually a different database on the same
        // server); WhereSQL() is already database-qualified.
        var cohortTable = _cohort.ExternalCohortTable.DiscoverCohortTable().GetFullyQualifiedName();
        var subquery = $"SELECT {_cohort.GetPrivateIdentifier(true)} FROM {cohortTable} WHERE {_cohort.WhereSQL()}";
        return (subquery, "", _cohort.GetCountDistinctFromDatabase(Timeout));
    }
}

/// <summary>
/// Breaks a live <see cref="CohortIdentificationConfiguration"/>'s build query down by health board.
/// </summary>
public class ExecuteCommandExportCicHealthBoardBreakdown : ExecuteCommandExportHealthBoardBreakdownBase
{
    private readonly CohortIdentificationConfiguration _cic;

    public ExecuteCommandExportCicHealthBoardBreakdown(IBasicActivateItems activator,
        [DemandsInitialization("The cohort identification configuration whose final inclusion list to break down")]
        CohortIdentificationConfiguration cic,
        [DemandsInitialization("CSV file to write. Defaults to <cic name>-healthboard.csv in the current directory")]
        FileInfo toFile = null,
        [DemandsInitialization("Name of the demography catalogue holding the region column", DefaultValue = "SHARE_Demography")]
        string demographyCatalogue = "SHARE_Demography",
        [DemandsInitialization("Name of the region (health board cipher) column on the demography catalogue", DefaultValue = "Region")]
        string regionColumn = "Region",
        [DemandsInitialization("Per-query command timeout in seconds", DefaultValue = 5000)]
        int timeout = 5000) : base(activator, toFile, demographyCatalogue, regionColumn, timeout)
    {
        _cic = cic;

        if (_cic == null)
            SetImpossible("No CohortIdentificationConfiguration was supplied");
        else if (_cic.RootCohortAggregateContainer_ID == null)
            SetImpossible($"'{_cic}' has no root container to run");
        else
            ResolveDemography();
    }

    protected override string SourceName => _cic.Name;

    protected override (string subquery, string paramSql, int distinctCount) GetCohortIdentifierSql()
    {
        // CIC: inline the live build query. DoNotWriteOutParameters keeps the DECLAREs out of the
        // SELECT so we can hoist them to the front of the whole batch.
        var builder = new CohortQueryBuilder(_cic, null) { DoNotWriteOutParameters = true };
        var body = builder.SQL;
        var paramSql = string.Concat(builder.ParameterManager.GetFinalResolvedParametersList()
            .Select(QueryBuilder.GetParameterDeclarationSQL));

        // distinct count = number of identifiers the build query yields (its output is already the
        // distinct identifier list); reuse the same hoisted parameters.
        return (body, paramSql, CountCohort(paramSql, body));
    }
}
