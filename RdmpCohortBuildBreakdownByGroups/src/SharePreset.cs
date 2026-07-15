// The SHARE deployment preset: well-known object NAMES resolved against the repository at runtime.
// This is deliberately the ONLY place any deployment-specific name lives - the generic engine
// (ExecuteCommandExportCohortBuildBreakDownByGroups) has no defaults at all. If a name is not found
// (or is ambiguous) the preset returns null for that input: required inputs are then prompted for by
// the command; an unresolved OPTIONAL grouping is simply omitted.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;

namespace RdmpCohortBuildBreakdownByGroups;

public static class SharePreset
{
    public const string ReferenceCatalogueName = "SHARE_Demography";
    public const string GroupColumnName = "Region";
    public const string LookupTableName = "z_hb_lookup";
    public const string LookupKeyColumnName = "Region";
    public const string LookupLabelColumnName = "HB_Name";
    public const string LookupGroupingColumnName = "SafeHaven_Region";

    /// <summary>
    /// Resolves the preset names against the repository. Any input that cannot be resolved to exactly
    /// one object comes back null (required inputs are then prompted for by the command; an unresolved
    /// optional grouping is simply omitted).
    /// </summary>
    public static void TryResolve(ICatalogueRepository repository,
        out ColumnInfo groupColumn, out ColumnInfo lookupKey, out ColumnInfo lookupLabel,
        out ColumnInfo lookupGrouping)
    {
        groupColumn = null;

        var cata = Single(repository.GetAllObjects<Catalogue>(),
            c => string.Equals(c.Name, ReferenceCatalogueName, StringComparison.OrdinalIgnoreCase));
        if (cata != null)
            groupColumn = Single(cata.GetAllExtractionInformation(ExtractionCategory.Any),
                    e => string.Equals(e.GetRuntimeName(), GroupColumnName, StringComparison.OrdinalIgnoreCase))
                ?.ColumnInfo;

        var lookupTable = Single(repository.GetAllObjects<TableInfo>(),
            t => string.Equals(t.GetRuntimeName(), LookupTableName, StringComparison.OrdinalIgnoreCase));

        lookupKey = LookupColumn(lookupTable, LookupKeyColumnName);
        lookupLabel = LookupColumn(lookupTable, LookupLabelColumnName);
        lookupGrouping = LookupColumn(lookupTable, LookupGroupingColumnName);
    }

    private static ColumnInfo LookupColumn(TableInfo table, string name) =>
        table == null
            ? null
            : Single(table.ColumnInfos,
                c => string.Equals(c.GetRuntimeName(), name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Exactly one match or null (ambiguity is treated as not found).</summary>
    private static T Single<T>(T[] available, Func<T, bool> predicate) where T : class
    {
        var matches = available.Where(predicate).Take(2).ToList();
        return matches.Count == 1 ? matches[0] : null;
    }
}
