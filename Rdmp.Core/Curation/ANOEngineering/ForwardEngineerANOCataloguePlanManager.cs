// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using Newtonsoft.Json;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.Sharing.Refactoring;

namespace Rdmp.Core.Curation.ANOEngineering;

/// <summary>
///     Configuration class for ForwardEngineerANOCatalogueEngine (See ForwardEngineerANOCatalogueEngine).  This class
///     stores which anonymisation transforms/dilutions
///     etc to apply to which columns, which TableInfos are to be mirated etc.  Also stores whether the LoadMetadata that
///     is to be created should be a single one off
///     load or should load in date based batches (e.g. 1 year at a time - use this option if you have too much data in the
///     source table to be migrated in one go - e.g.
///     tens of millions of records).
/// </summary>
public class ForwardEngineerANOCataloguePlanManager : ICheckable, IPickAnyConstructorFinishedCallback
{
    private readonly ShareManager _shareManager;

    public ICatalogue Catalogue
    {
        get => _catalogue;
        set
        {
            _catalogue = value;
            RefreshTableInfos();
        }
    }

    private ExtractionInformation[] _allExtractionInformations;
    private CatalogueItem[] _allCatalogueItems;

    public Dictionary<ColumnInfo, ColumnInfoANOPlan> Plans = new();

    [JsonIgnore] public List<IDilutionOperation> DilutionOperations { get; }

    public ITableInfo[] TableInfos { get; private set; }

    [JsonIgnore] public DiscoveredDatabase TargetDatabase { get; set; }

    public ColumnInfo DateColumn { get; set; }
    public DateTime? StartDate { get; set; }

    [JsonIgnore] public HashSet<ITableInfo> SkippedTables = new();
    private ICatalogue _catalogue;

    /// <summary>
    ///     This constructor is primarily intended for deserialization via
    ///     <see cref="JsonConvertExtensions.DeserializeObject" />.  You should
    ///     instead use the overload.
    /// </summary>
    public ForwardEngineerANOCataloguePlanManager(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        _shareManager = new ShareManager(repositoryLocator);

        DilutionOperations = new List<IDilutionOperation>();

        foreach (var operationType in MEF.GetTypes<IDilutionOperation>())
            DilutionOperations.Add((IDilutionOperation)ObjectConstructor.Construct(operationType));
    }

    public ForwardEngineerANOCataloguePlanManager(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ICatalogue catalogue) : this(repositoryLocator)
    {
        Catalogue = catalogue;

        foreach (var plan in Plans.Values)
            plan.SetToRecommendedPlan();
    }

    public ColumnInfoANOPlan GetPlanForColumnInfo(ColumnInfo col)
    {
        return !Plans.TryGetValue(col, out var anoPlan)
            ? throw new Exception($"No plan found for column {col}")
            : anoPlan;
    }

    public IExternalDatabaseServer GetIdentifierDumpServer()
    {
        return Catalogue.CatalogueRepository.GetDefaultFor(PermissableDefaults.IdentifierDumpServer_ID);
    }


    public void Check(ICheckNotifier notifier)
    {
        if (TargetDatabase == null)
            notifier.OnCheckPerformed(new CheckEventArgs("No TargetDatabase has been set", CheckResult.Fail));
        else if (!TargetDatabase.Exists())
            notifier.OnCheckPerformed(new CheckEventArgs($"TargetDatabase '{TargetDatabase}' does not exist",
                CheckResult.Fail));

        var toMigrateTables = TableInfos.Except(SkippedTables).ToArray();

        if (!toMigrateTables.Any())
            notifier.OnCheckPerformed(new CheckEventArgs("There are no TableInfos selected for anonymisation",
                CheckResult.Fail));

        try
        {
            var joinInfos = GetJoinInfosRequiredCatalogue();
            notifier.OnCheckPerformed(new CheckEventArgs("Generated Catalogue SQL successfully", CheckResult.Success));

            foreach (var joinInfo in joinInfos)
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Found required JoinInfo '{joinInfo}' that will have to be migrated", CheckResult.Success));

            foreach (var lookup in GetLookupsRequiredCatalogue())
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Found required Lookup '{lookup}' that will have to be migrated", CheckResult.Success));

                //for each key involved in the lookup
                foreach (var c in new[] { lookup.ForeignKey, lookup.PrimaryKey, lookup.Description })
                {
                    //lookup / table has already been migrated
                    if (SkippedTables.Any(t => t.ID == c.TableInfo_ID))
                        continue;

                    //make sure that the plan is sensible
                    if (GetPlanForColumnInfo(c).Plan != Plan.PassThroughUnchanged)
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            $"ColumnInfo '{c}' is part of a Lookup so must PassThroughUnchanged", CheckResult.Fail));
                }
            }
        }
        catch (Exception ex)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Failed to generate Catalogue SQL", CheckResult.Fail, ex));
        }

        if (DateColumn != null)
        {
            var dateColumnPlan = GetPlanForColumnInfo(DateColumn);
            if (dateColumnPlan.Plan != Plan.PassThroughUnchanged)
                if (notifier.OnCheckPerformed(new CheckEventArgs($"Plan for {DateColumn} must be PassThroughUnchanged",
                        CheckResult.Fail, null, "Set plan to PassThroughUnchanged")))
                    dateColumnPlan.Plan = Plan.PassThroughUnchanged;

            //get a count of the number of non lookup used tables
            var usedTables = TableInfos.Except(SkippedTables).Count(t => !t.IsLookupTable());

            if (usedTables > 1)
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"You cannot have a date based migration because you are trying to migrate {usedTables} TableInfos at once",
                        CheckResult.Fail));
        }

        if (Plans.Any(p => p.Value.Plan == Plan.Dilute))
            if (GetIdentifierDumpServer() == null)
                notifier.OnCheckPerformed(new CheckEventArgs("No default Identifier Dump server has been configured",
                    CheckResult.Fail));

        var refactorer = new SelectSQLRefactorer();

        foreach (var e in _allExtractionInformations)
            if (!SelectSQLRefactorer.IsRefactorable(e))
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"ExtractionInformation '{e}' is a not refactorable due to reason:{SelectSQLRefactorer.GetReasonNotRefactorable(e)}",
                    CheckResult.Fail));

        notifier.OnCheckPerformed(new CheckEventArgs(
            $"Preparing to evaluate {toMigrateTables.Length}' tables ({string.Join(",", toMigrateTables.Select(t => t.GetFullyQualifiedName()))})",
            CheckResult.Success));

        foreach (TableInfo tableInfo in toMigrateTables)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Evaluating TableInfo '{tableInfo}'", CheckResult.Success));

            if (TargetDatabase != null && TargetDatabase.ExpectTable(tableInfo.GetRuntimeName()).Exists())
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Table '{tableInfo}' already exists in Database '{TargetDatabase}'", CheckResult.Fail));

            var pks = tableInfo.ColumnInfos.Where(c => c.IsPrimaryKey).ToArray();

            if (!pks.Any())
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"TableInfo '{tableInfo}' does not have any Primary Keys, it cannot be anonymised",
                    CheckResult.Fail));

            if (tableInfo.IsTableValuedFunction)
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"TableInfo '{tableInfo}' is an IsTableValuedFunction so cannot be anonymised", CheckResult.Fail));

            EnsureNotAlreadySharedLocally(notifier, tableInfo);
            EnsureNotAlreadySharedLocally(notifier, Catalogue);
        }

        //check the column level plans
        foreach (var p in Plans.Values)
            p.Check(notifier);
    }

    private void EnsureNotAlreadySharedLocally<T>(ICheckNotifier notifier, T m) where T : IMapsDirectlyToDatabaseTable
    {
        if (_shareManager.IsExportedObject(m))
        {
            var existingExport = _shareManager.GetNewOrExistingExportFor(m);
            var existingImportReference = _shareManager.GetExistingImport(existingExport.SharingUID);

            if (existingImportReference != null)
            {
                var existingImportInstance = m.Repository.GetObjectByID<T>(existingImportReference.ReferencedObjectID);
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"{typeof(T)} '{m}' is already locally shared as '{existingImportInstance}'", CheckResult.Fail));
            }
        }
    }

    /// <summary>
    ///     Re checks the TableInfos associated with the Catalogue incase some have changed
    /// </summary>
    public void RefreshTableInfos()
    {
        TableInfos = Catalogue.GetTableInfoList(true);

        //generate plans for novel ColumnInfos
        foreach (TableInfo tableInfo in TableInfos)
        foreach (var columnInfo in tableInfo.ColumnInfos)
            if (!Plans.ContainsKey(columnInfo))
                Plans.Add(columnInfo, new ColumnInfoANOPlan(columnInfo));


        //Remove unplanned columns
        foreach (var col in Plans.Keys.ToArray())
            if (!IsStillNeeded(col))
                Plans.Remove(col);

        InitializePlans();
    }

    private void InitializePlans()
    {
        _allExtractionInformations = Catalogue.GetAllExtractionInformation(ExtractionCategory.Any);
        _allCatalogueItems = Catalogue.CatalogueItems.Where(ci => ci.ColumnInfo_ID != null).ToArray();
        var allColumnInfosSystemWide = Catalogue.Repository.GetAllObjects<ColumnInfo>();
        var joins = GetJoinInfosRequiredCatalogue();
        var lookups = GetLookupsRequiredCatalogue();

        foreach (var plan in Plans.Values)
            plan.Initialize(_allExtractionInformations, _allCatalogueItems, joins, lookups, allColumnInfosSystemWide,
                this);
    }

    public List<JoinInfo> GetJoinInfosRequiredCatalogue()
    {
        var qb = new QueryBuilder(null, null);
        qb.AddColumnRange(Catalogue.GetAllExtractionInformation(ExtractionCategory.Any));
        qb.RegenerateSQL();
        return qb.JoinsUsedInQuery;
    }

    public List<Lookup> GetLookupsRequiredCatalogue()
    {
        var qb = new QueryBuilder(null, null);
        qb.AddColumnRange(Catalogue.GetAllExtractionInformation(ExtractionCategory.Any));
        qb.RegenerateSQL();

        return qb.GetDistinctRequiredLookups().ToList();
    }

    private bool IsStillNeeded(ColumnInfo columnInfo)
    {
        return TableInfos.Any(t => t.ID == columnInfo.TableInfo_ID);
    }

    public void AfterConstruction()
    {
        InitializePlans();
    }
}