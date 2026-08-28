// The SHARE deployment preset: well-known object NAMES resolved against the repository at runtime.
// This is deliberately the ONLY place any deployment-specific name lives - the generic engine
// (ExecuteCommandExportCohortBuildBreakDownByGroups) has no defaults at all. If a name is not found
// (or is ambiguous) the preset returns null for that input and the command prompts for it.

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

    /// <summary>
    /// Resolves the preset names against the repository. Any input that cannot be resolved to exactly
    /// one object comes back null (the command then prompts for it). When several tables share the
    /// lookup name (e.g. copies imported from other databases), the one co-located with the reference
    /// table wins - the command requires them on the same server anyway.
    /// </summary>
    public static void TryResolve(ICatalogueRepository repository,
        out ColumnInfo groupColumn, out ColumnInfo lookupKey, out ColumnInfo lookupLabel)
    {
        ColumnInfo group = null;

        var cata = Single(repository.GetAllObjects<Catalogue>(),
            c => string.Equals(c.Name, ReferenceCatalogueName, StringComparison.OrdinalIgnoreCase));
        if (cata != null)
            group = Single(cata.GetAllExtractionInformation(ExtractionCategory.Any),
                    e => string.Equals(e.GetRuntimeName(), GroupColumnName, StringComparison.OrdinalIgnoreCase))
                ?.ColumnInfo;

        var candidates = repository.GetAllObjects<TableInfo>()
            .Where(t => string.Equals(t.GetRuntimeName(), LookupTableName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        // disambiguate by co-location with the reference table: same server AND database first,
        // then same server; only give up (prompt) if still not exactly one
        if (candidates.Length > 1 && group != null)
        {
            var sameServerAndDb = candidates.Where(t =>
                    SameName(t.Server, group.TableInfo.Server) && SameName(t.Database, group.TableInfo.Database))
                .ToArray();
            var sameServer = candidates.Where(t => SameName(t.Server, group.TableInfo.Server)).ToArray();

            candidates = sameServerAndDb.Length == 1 ? sameServerAndDb
                : sameServer.Length == 1 ? sameServer
                : candidates;
        }

        var lookupTable = candidates.Length == 1 ? candidates[0] : null;

        groupColumn = group;
        lookupKey = LookupColumn(lookupTable, LookupKeyColumnName);
        lookupLabel = LookupColumn(lookupTable, LookupLabelColumnName);
    }

    private static bool SameName(string a, string b) =>
        string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);

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
