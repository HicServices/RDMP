// Rebuilds a cohort from a build.script.yaml produced by ExecuteCommandExportCohortAsScript.
// Replays each command in-process, binding the $c/$a/$p handles to the real ids RDMP assigns
// as each object is created (via NewObjectPool), deletes the auto Inclusion/Exclusion
// containers, and restores child Order (each Add inserts at the top, which would reverse it).
//
//   rdmp cmd BuildCohortFromScript .\out\MyCohort\build.script.yaml "MyCohort (rebuilt)"
//
// This is the inverse of ExportCohortAsScript. Verified to reproduce identical membership.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using YamlDotNet.Serialization;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortScript;

public class ExecuteCommandBuildCohortFromScript : BasicCommandExecution
{
    private readonly FileInfo _scriptFile;
    private readonly string _newName;
    private readonly Dictionary<string, int> _handles = new(); // "$c25" -> real id
    private readonly Dictionary<int, int> _ixMap = new();      // old joinable id -> new (for ix#### alias)
    private CohortIdentificationConfiguration _currentCic;

    public ExecuteCommandBuildCohortFromScript(IBasicActivateItems activator,
        [DemandsInitialization("The build.script.yaml produced by ExportCohortAsScript")]
        FileInfo scriptFile,
        [DemandsInitialization("Name for the rebuilt cohort (must be unique)")]
        string newCohortName = null) : base(activator)
    {
        _scriptFile = scriptFile;
        _newName = newCohortName;
        if (scriptFile != null && !scriptFile.Exists)
            SetImpossible("Script file does not exist"); // null is allowed - the GUI prompts
    }

    private class ScriptDto { public string[] Commands { get; set; } }

    public override void Execute()
    {
        base.Execute();

        // From the GUI the file/name may be unset - prompt for them.
        var file = _scriptFile ?? BasicActivator.SelectFile("Select build.script.yaml to rebuild", "Cohort script", "*.yaml");
        if (file == null) return;
        var newName = _newName;
        if (string.IsNullOrWhiteSpace(newName) &&
            (!BasicActivator.TypeText("Rebuild Cohort", "Name for the rebuilt cohort", 200, null, out newName, false)
             || string.IsNullOrWhiteSpace(newName)))
            return;

        var script = new Deserializer().Deserialize<ScriptDto>(File.ReadAllText(file.FullName));
        if (script?.Commands == null || script.Commands.Length == 0)
            throw new Exception("Script contained no Commands");

        var repo = BasicActivator.RepositoryLocator.CatalogueRepository;
        var invoker = new CommandInvoker(BasicActivator);
        var byName = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var t in invoker.GetSupportedCommands())
            byName.TryAdd(BasicCommandExecution.GetCommandName(t.Name), t);

        // (parentContainerId, isAggregate, childId) in the order children were added
        var adds = new List<(int parent, bool isAgg, int child)>();

        using (NewObjectPool.StartSession())
        {
            for (var cmdIndex = 0; cmdIndex < script.Commands.Length; cmdIndex++)
            {
                var line = script.Commands[cmdIndex];
                string binds = null;
                var i = line.IndexOf(" => ", StringComparison.Ordinal);
                if (i >= 0) { binds = line[(i + 4)..].Trim(); line = line[..i]; }

                if (line.StartsWith("CreateNewCohortIdentificationConfiguration", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(newName))
                    line = $"CreateNewCohortIdentificationConfiguration \"{newName}\"";

                line = Substitute(line);

                // rewrite patient-index-table aliases ix<oldJoinableId> -> ix<newJoinableId>
                // (the join alias is instance-specific; the PIT was created earlier this run).
                foreach (var kv in _ixMap)
                    line = line.Replace($"ix{kv.Key}.", $"ix{kv.Value}.");

                // runner directive: create a patient index table (joinable) from a catalogue, with
                // the given non-identifier dimensions, then convert it to a PIT.
                // CreatePatientIndexTable Catalogue:<name> Dimensions:"col1,col2" => $pit<oldJoinableId>
                if (line.StartsWith("CreatePatientIndexTable", StringComparison.OrdinalIgnoreCase))
                {
                    var t = Tokenize(line); // CreatePatientIndexTable Catalogue:<n> Aggregate:<oldAggId> Dimensions:"a,b"
                    var cataName = Field(t, "Catalogue:");
                    var oldAggId = Field(t, "Aggregate:");
                    var dimCols = Field(t, "Dimensions:").Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var cata = repo.GetAllObjects<Catalogue>().First(c => c.Name == cataName);

                    var aggCmd = new CatalogueCombineable(cata).GenerateAggregateConfigurationFor(BasicActivator, _currentCic);
                    foreach (var col in dimCols)
                    {
                        var ei = cata.GetAllExtractionInformation().FirstOrDefault(e => e.GetRuntimeName() == col);
                        if (ei != null) _ = new AggregateDimension(repo, ei, aggCmd.Aggregate);
                    }
                    // bind the PIT aggregate's $a<oldId> so later SetDimensionSql overrides can target it
                    if (oldAggId != null) _handles[$"$a{oldAggId}"] = aggCmd.Aggregate.ID;
                    new ExecuteCommandConvertAggregateConfigurationToPatientIndexTable(BasicActivator, aggCmd, _currentCic).Execute();

                    var joinable = (JoinableCohortAggregateConfiguration)NewObjectPool.Latest(
                        repo.GetAllObjects<JoinableCohortAggregateConfiguration>());
                    if (binds != null)
                    {
                        _handles[binds] = joinable.ID;             // $pit<old> -> new joinable id
                        _ixMap[int.Parse(binds[4..])] = joinable.ID; // old id (after "$pit") -> new
                    }
                    continue;
                }

                // runner directive: restore a dimension's customised SelectSQL (e.g. the extraction
                // identifier qualified to [db]..[tbl].[col] so a PIT join's chi isn't ambiguous).
                // SetDimensionSql <aggId> "<runtimeName>" "<selectSql>"
                if (line.StartsWith("SetDimensionSql", StringComparison.OrdinalIgnoreCase))
                {
                    var t = Tokenize(line);
                    var dim = FindDimension(repo, int.Parse(t[1]), t[2]);
                    if (dim != null) { dim.SelectSQL = t[3]; dim.SaveToDatabase(); }
                    continue;
                }

                // runner directive: restore a dimension's column Alias. Functional for patient
                // index tables: the joining filter references ix<id>.<alias>.
                // SetDimensionAlias <aggId> "<underlyingColumn>" "<alias>"
                if (line.StartsWith("SetDimensionAlias", StringComparison.OrdinalIgnoreCase))
                {
                    var t = Tokenize(line);
                    var dim = FindDimension(repo, int.Parse(t[1]), t[2]);
                    if (dim != null) { dim.Alias = t[3]; dim.SaveToDatabase(); }
                    continue;
                }

                // runner directive: make a cohort set join to a patient index table.
                // UsePatientIndexTable <setAggId> <joinableId> <JoinType>
                if (line.StartsWith("UsePatientIndexTable", StringComparison.OrdinalIgnoreCase))
                {
                    var t = Tokenize(line);
                    var setAgg = repo.GetObjectByID<AggregateConfiguration>(int.Parse(t[1]));
                    var joinable = repo.GetObjectByID<JoinableCohortAggregateConfiguration>(int.Parse(t[2]));
                    var use = joinable.AddUser(setAgg);
                    use.JoinType = Enum.Parse<ExtractionJoinType>(t[3], true);
                    use.SaveToDatabase();
                    continue;
                }

                // runner directive: associate the new CIC with a Project (so project-specific
                // catalogues can be added). Handled directly, not via a command.
                if (line.StartsWith("AssociateWithProject", StringComparison.OrdinalIgnoreCase))
                {
                    var pid = int.Parse(line[(line.IndexOf("Project:", StringComparison.Ordinal) + 8)..].Trim());
                    var dx = BasicActivator.RepositoryLocator.DataExportRepository;
                    _ = new ProjectCohortIdentificationConfigurationAssociation(dx, dx.GetObjectByID<Project>(pid), _currentCic);
                    continue;
                }

                // SetContainerOperation on a NAMED cohort container (Root/Inclusion/Exclusion)
                // prompts for a rename and fails headless - set it directly instead.
                if (line.StartsWith("SetContainerOperation CohortAggregateContainer:", StringComparison.OrdinalIgnoreCase))
                {
                    var t = Tokenize(line);
                    var cont = repo.GetObjectByID<CohortAggregateContainer>(int.Parse(t[1][(t[1].IndexOf(':') + 1)..]));
                    cont.Operation = Enum.Parse<SetOperation>(t[2], true);
                    cont.SaveToDatabase();
                    continue;
                }

                // runner directive: aggregate-level parameter.
                // AddAggregateParameter <aggId> "name" "DECLARE @x AS type" "value"
                if (line.StartsWith("AddAggregateParameter", StringComparison.OrdinalIgnoreCase))
                {
                    var t = Tokenize(line); // [cmd, aggId, name, parameterSQL, value]
                    var agg = repo.GetObjectByID<AggregateConfiguration>(int.Parse(t[1]));
                    new AnyTableSqlParameter(repo, agg, t[3]) { Value = t[4] }.SaveToDatabase();
                    continue;
                }

                // runner directive: a parameter on a HAND-WRITTEN filter (imported filters get
                // theirs auto-created, but CreateNewFilter "name" "sql" does not).
                // AddFilterParameter <filterId> "name" "DECLARE @x AS type" "value"
                if (line.StartsWith("AddFilterParameter", StringComparison.OrdinalIgnoreCase))
                {
                    var t = Tokenize(line); // [cmd, filterId, name, parameterSQL, value]
                    var filter = repo.GetObjectByID<AggregateFilter>(int.Parse(t[1]));
                    var p = (AggregateFilterParameter)filter.GetFilterFactory().CreateNewParameter(filter, t[3]);
                    p.Value = t[4];
                    p.SaveToDatabase();
                    continue;
                }

                // runner directive: cohort-level (global) parameter on the CIC itself.
                // AddGlobalParameter "name" "DECLARE @x AS type" "value"
                if (line.StartsWith("AddGlobalParameter", StringComparison.OrdinalIgnoreCase))
                {
                    var t = Tokenize(line); // [cmd, name, parameterSQL, value]
                    new AnyTableSqlParameter(repo, _currentCic, t[2]) { Value = t[3] }.SaveToDatabase();
                    continue;
                }

                // runner directive: force a table into an aggregate's query (AggregateForcedJoin).
                // AddForcedJoin <aggId> TableInfo:<fully qualified name>
                if (line.StartsWith("AddForcedJoin", StringComparison.OrdinalIgnoreCase))
                {
                    var t = Tokenize(line);
                    var agg = repo.GetObjectByID<AggregateConfiguration>(int.Parse(t[1]));
                    var tiName = Field(t, "TableInfo:");
                    var ti = repo.GetAllObjects<TableInfo>().FirstOrDefault(x => x.Name == tiName);
                    if (ti != null) repo.AggregateForcedJoinManager.CreateLinkBetween(agg, ti);
                    continue;
                }

                // runner directive: make the aggregate's root filter container with an AND/OR op.
                // EnsureFilterContainer <aggId> <AND|OR> => $fcN
                if (line.StartsWith("EnsureFilterContainer", StringComparison.OrdinalIgnoreCase))
                {
                    var t = Tokenize(line);
                    var agg = repo.GetObjectByID<AggregateConfiguration>(int.Parse(t[1]));
                    int fcid;
                    if (agg.RootFilterContainer_ID == null)
                    {
                        var nfc = new AggregateFilterContainer(repo, Enum.Parse<FilterContainerOperation>(t[2], true));
                        agg.RootFilterContainer_ID = nfc.ID;
                        agg.SaveToDatabase();
                        fcid = nfc.ID;
                    }
                    else fcid = agg.RootFilterContainer_ID.Value;
                    if (binds != null) _handles[binds] = fcid;
                    continue;
                }

                // runner directive: add an AND/OR sub-container under a filter container.
                // AddFilterSubContainer <parentFcId> <AND|OR> => $fcN
                if (line.StartsWith("AddFilterSubContainer", StringComparison.OrdinalIgnoreCase))
                {
                    var t = Tokenize(line);
                    var parent = repo.GetObjectByID<AggregateFilterContainer>(int.Parse(t[1]));
                    var sub = new AggregateFilterContainer(repo, Enum.Parse<FilterContainerOperation>(t[2], true));
                    parent.AddChild(sub);
                    if (binds != null) _handles[binds] = sub.ID;
                    continue;
                }

                var tokens = Tokenize(line);
                if (!byName.TryGetValue(tokens[0], out var type))
                    throw new Exception($"Unknown command '{tokens[0]}'");

                invoker.ExecuteCommand(type, new CommandLineObjectPicker(tokens.Skip(1).ToArray(), BasicActivator));

                BindAndTrack(tokens[0], line, binds, repo, adds);
            }

            RestoreOrder(repo, adds);
        }

        var cic = repo.GetAllObjectsWhere<CohortIdentificationConfiguration>("Name", newName).FirstOrDefault();
        BasicActivator.Show($"Rebuilt cohort '{newName}'" + (cic != null ? $" (ID {cic.ID})" : ""));
        if (cic != null) Publish(cic);
    }

    private void BindAndTrack(string cmd, string line, string binds, ICatalogueRepository repo,
        List<(int, bool, int)> adds)
    {
        switch (cmd)
        {
            case "CreateNewCohortIdentificationConfiguration":
                var cic = (CohortIdentificationConfiguration)NewObjectPool.Latest(
                    repo.GetAllObjects<CohortIdentificationConfiguration>());
                _currentCic = cic;
                var root = cic.RootCohortAggregateContainer;
                if (root != null)
                {
                    if (binds != null) _handles[binds] = root.ID;
                    // remove the auto Inclusion/Exclusion containers (the script recreates what it needs)
                    foreach (var sub in root.GetSubContainers()
                                 .Where(s => s.Name is "Inclusion Criteria" or "Exclusion Criteria"))
                        sub.DeleteInDatabase();
                }
                break;

            case "AddCohortSubContainer":
                var newC = (CohortAggregateContainer)NewObjectPool.Latest(
                    repo.GetAllObjects<CohortAggregateContainer>());
                if (binds != null) _handles[binds] = newC.ID;
                adds.Add((ParentContainerId(line), false, newC.ID));
                break;

            case "AddCatalogueToCohortIdentificationSetContainer":
                var newA = (AggregateConfiguration)NewObjectPool.Latest(
                    repo.GetAllObjects<AggregateConfiguration>());
                if (binds != null) _handles[binds] = newA.ID;
                adds.Add((ParentContainerId(line), true, newA.ID));
                break;

            case "CreateNewFilter":
                var filter = (AggregateFilter)NewObjectPool.Latest(repo.GetAllObjects<AggregateFilter>());
                var handles = (binds ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (handles.Length == 1 && handles[0].StartsWith("$f", StringComparison.Ordinal))
                {
                    // hand-written filter: bind the FILTER so AddFilterParameter can create its params
                    _handles[handles[0]] = filter.ID;
                }
                else
                {
                    // imported filter: params were auto-created; bind their handles by position
                    var ps = filter.GetAllParameters().OfType<AggregateFilterParameter>().ToArray();
                    for (var k = 0; k < handles.Length && k < ps.Length; k++)
                        _handles[handles[k]] = ps[k].ID;
                }
                break;
        }
    }

    private static void RestoreOrder(ICatalogueRepository repo, List<(int parent, bool isAgg, int child)> adds)
    {
        foreach (var grp in adds.GroupBy(a => a.parent))
        {
            var n = 0;
            foreach (var (_, isAgg, child) in grp)
            {
                if (isAgg)
                    repo.CohortContainerManager.SetOrder(repo.GetObjectByID<AggregateConfiguration>(child), n);
                else
                {
                    var c = repo.GetObjectByID<CohortAggregateContainer>(child);
                    c.Order = n;
                    c.SaveToDatabase();
                }
                n++;
            }
        }
    }

    private string Substitute(string line)
    {
        foreach (var kv in _handles.OrderByDescending(k => k.Key.Length))
            line = line.Replace(kv.Key, kv.Value.ToString());
        return line;
    }

    private static int ParentContainerId(string line)
    {
        // first CohortAggregateContainer:<id> in the (already-substituted) line
        const string key = "CohortAggregateContainer:";
        var at = line.IndexOf(key, StringComparison.Ordinal);
        if (at < 0) return -1;
        var s = at + key.Length;
        var e = s;
        while (e < line.Length && char.IsDigit(line[e])) e++;
        return int.TryParse(line[s..e], out var id) ? id : -1;
    }

    // Finds an aggregate's dimension by its UNDERLYING column name (stable across aliasing);
    // falls back to the dimension's runtime name for scripts that predate alias support.
    private static AggregateDimension FindDimension(ICatalogueRepository repo, int aggId, string column)
    {
        var agg = repo.GetObjectByID<AggregateConfiguration>(aggId);
        return agg.AggregateDimensions.FirstOrDefault(d => d.ExtractionInformation?.GetRuntimeName() == column)
               ?? agg.AggregateDimensions.FirstOrDefault(d => d.GetRuntimeName() == column);
    }

    // value of a "Key:value" token (quotes already stripped by Tokenize), or null if absent
    private static string Field(List<string> tokens, string key)
    {
        var tok = tokens.FirstOrDefault(x => x.StartsWith(key, StringComparison.OrdinalIgnoreCase));
        return tok?[key.Length..];
    }

    // split on spaces, honouring double quotes anywhere in a token; quote chars are dropped
    private static List<string> Tokenize(string line)
    {
        var tokens = new List<string>();
        var sb = new StringBuilder();
        var inQuote = false;
        var has = false;
        foreach (var ch in line)
        {
            if (ch == '"') { inQuote = !inQuote; has = true; }
            else if (ch == ' ' && !inQuote)
            {
                if (has) { tokens.Add(sb.ToString()); sb.Clear(); has = false; }
            }
            else { sb.Append(ch); has = true; }
        }
        if (has) tokens.Add(sb.ToString());
        return tokens;
    }
}
