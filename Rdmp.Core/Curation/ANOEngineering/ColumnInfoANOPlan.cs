// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Discovery.TypeTranslation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.ReusableLibraryCode.Checks;
using TypeGuesser;

namespace Rdmp.Core.Curation.ANOEngineering;

/// <summary>
///     Records all ANO descisions made about a single ColumnInfo (e.g. whether to ANO it, Drop it, Dillute it etc)
/// </summary>
public class ColumnInfoANOPlan : ICheckable
{
    public ColumnInfo ColumnInfo
    {
        get => _columnInfo;
        set
        {
            _columnInfo = value;
            if (value != null)
                _querySyntaxHelper = _columnInfo.GetQuerySyntaxHelper();
        }
    }

    private ExtractionInformation[] _allExtractionInformations;
    private CatalogueItem[] _allCatalogueItems;
    private List<JoinInfo> _joins;
    private List<Lookup> _lookups;
    private ColumnInfo[] _allColumnInfosSystemWide;
    private ForwardEngineerANOCataloguePlanManager _planManager;
    private Plan _plan;
    private ColumnInfo _columnInfo;
    private IQuerySyntaxHelper _querySyntaxHelper;

    public ANOTable ANOTable { get; set; }
    public IDilutionOperation Dilution { get; set; }
    public ExtractionCategory? ExtractionCategoryIfAny { get; set; }

    public Plan Plan
    {
        get => _plan;
        set
        {
            _plan = value;

            switch (value)
            {
                case Plan.Drop:
                    ANOTable = null;
                    Dilution = null;
                    ExtractionCategoryIfAny = null;
                    break;
                case Plan.ANO:
                    Dilution = null;
                    break;
                case Plan.Dilute:
                    ANOTable = null;
                    break;
                case Plan.PassThroughUnchanged:
                    ANOTable = null;
                    Dilution = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }

    public bool IsMandatory =>
        //ColumnInfo is part of the table primary key
        ColumnInfo.IsPrimaryKey;

    /// <summary>
    ///     Json deserialization constructor <see cref="PickAnyConstructorJsonConverter" />
    /// </summary>
    public ColumnInfoANOPlan(ColumnInfo columnInfo)
    {
        ColumnInfo = columnInfo;
    }

    /// <summary>
    ///     Make up ANOTable plans based on existing ANOTable/column usages.  For example if the column chi is being migrated
    ///     and there is at least one column
    ///     called chi or ANOchi already existing (probably from another table) then we should suggest using ANOTable ANOchi.
    /// </summary>
    /// <returns></returns>
    internal void Initialize(ExtractionInformation[] allExtractionInformations, CatalogueItem[] allCatalogueItems,
        List<JoinInfo> joins, List<Lookup> lookups, ColumnInfo[] allColumnInfosSystemWide,
        ForwardEngineerANOCataloguePlanManager planManager)
    {
        _allExtractionInformations = allExtractionInformations;
        _allCatalogueItems = allCatalogueItems;
        _joins = joins;
        _lookups = lookups;
        _allColumnInfosSystemWide = allColumnInfosSystemWide;
        _planManager = planManager;
    }

    public void SetToRecommendedPlan()
    {
        //get an extraction category based on its current extractability
        ExtractionCategoryIfAny = GetMaxExtractionCategoryIfAny();

        if (SpecialFieldNames.IsHicPrefixed(ColumnInfo))
            Plan = Plan.Drop; //suggest dropping hic_ fields
        else if (ColumnInfo.IsPrimaryKey || IsInvolvedInLookups() || IsInvolvedInJoins())
            Plan = Plan.PassThroughUnchanged; //if it's involved in lookups, joins or is a primary key
        else if (ExtractionCategoryIfAny == null) //if it isn't extractable
            Plan = Plan.Drop;
        else
            Plan = Plan.PassThroughUnchanged; //it is extractable but not special


        //if there's an associated ANOTable with a different ColumnInfo with the same name e.g. chi=>ANOChi in another dataset that was already anonymised
        MakeANOTableSuggestionIfApplicable();
    }


    private ExtractionCategory? GetMaxExtractionCategoryIfAny()
    {
        ExtractionCategory? toReturn = null;

        //Decide on an ExtractionCategory
        //setup the planned extraction category based on what CatalogueItem(s) exist for it (1 to many)
        foreach (var catalogueItem in _allCatalogueItems.Where(ci => ci.ColumnInfo_ID == ColumnInfo.ID).ToArray())
        {
            //that are extractable
            var extractionInformation =
                _allExtractionInformations.SingleOrDefault(ei => ei.CatalogueItem_ID == catalogueItem.ID);

            //The ColumnInfo is extractable
            if (extractionInformation != null)
            {
                toReturn ??= extractionInformation.ExtractionCategory;

                //there are multiple, if the new one is more restrictive then use the more restrictive category instead
                toReturn = extractionInformation.ExtractionCategory > toReturn
                    ? extractionInformation.ExtractionCategory
                    : toReturn;
            }
        }

        return toReturn;
    }

    private void MakeANOTableSuggestionIfApplicable()
    {
        //if there is a ColumnInfo with the same name (or that has ANO prefix)
        var matchingOnName = _allColumnInfosSystemWide.Where(a => a.GetRuntimeName() == ColumnInfo.GetRuntimeName() ||
                                                                  a.GetRuntimeName() ==
                                                                  $"ANO{ColumnInfo.GetRuntimeName()}").ToArray();

        //and if the same named ColumnInfo(s) have a shared ANOTable (e.g. ANOCHI)
        var agreedAnoTableID = matchingOnName.Where(c => c.ANOTable_ID != null).Select(c => c.ANOTable_ID).Distinct()
            .ToArray();

        //if there is a single recommended anotable id amongst all columns with matching name featuring ano tables
        if (agreedAnoTableID.Length == 1)
        {
            ANOTable = ColumnInfo.Repository.GetObjectByID<ANOTable>(agreedAnoTableID.Single().Value);
            Plan = Plan.ANO;
        }
    }

    public bool IsInvolvedInLookups()
    {
        return _lookups.Any(l =>
            l.Description_ID == ColumnInfo.ID || l.ForeignKey_ID == ColumnInfo.ID || l.PrimaryKey_ID == l.ID);
    }

    private bool IsInvolvedInJoins()
    {
        return _joins.Any(j => j.PrimaryKey_ID == ColumnInfo.ID || j.ForeignKey_ID == ColumnInfo.ID);
    }

    public void Check(ICheckNotifier notifier)
    {
        if (IsMandatory && Plan == Plan.Drop)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"ColumnInfo '{ColumnInfo}' is mandatory and cannot be dropped", CheckResult.Fail));

        if (Plan == Plan.ANO && ANOTable == null)
            notifier.OnCheckPerformed(new CheckEventArgs($"No ANOTable has been picked for ColumnInfo '{ColumnInfo}'",
                CheckResult.Fail));

        if (Plan == Plan.Dilute && Dilution == null)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"No Dilution Operation has been picked for ColumnInfo '{ColumnInfo}'", CheckResult.Fail));

        if (Plan != Plan.Drop)
            try
            {
                var datatype = GetEndpointDataType();
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Determined endpoint data type '{datatype}' for ColumnInfo '{ColumnInfo}'", CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Could not determine endpoint data type for ColumnInfo '{ColumnInfo}'", CheckResult.Fail, e));
            }

        //don't let user select ExtractionCategory.Any
        if (ExtractionCategoryIfAny == ExtractionCategory.Any)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Extraction Category '{ExtractionCategoryIfAny}' is not valid (on ColumnInfo {ColumnInfo})",
                CheckResult.Fail));

        //for each column we are not planning on dropping but are planning on making extractable, there must be a CatalogueItem in the source Catalogue
        if (ExtractionCategoryIfAny != null && Plan != Plan.Drop)
            if (_allCatalogueItems.All(ci => ci.ColumnInfo_ID != ColumnInfo.ID))
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"There are no CatalogueItems configured for ColumnInfo '{ColumnInfo}' but its PlannedExtractionCategory is '{ExtractionCategoryIfAny}'",
                        CheckResult.Fail));

        //Will there be conflicts on name?
        if (_planManager.TargetDatabase != null && Plan != Plan.Drop)
            //don't let them extract to the same database
            if (_planManager.TargetDatabase.GetRuntimeName() == ColumnInfo.TableInfo.Database)
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"ColumnInfo {ColumnInfo} is already in {_planManager.TargetDatabase.GetRuntimeName()} you cannot create an ANO version in the same database",
                    CheckResult.Fail));
    }

    public string GetEndpointDataType()
    {
        var sourceTypeTranslater = _querySyntaxHelper.TypeTranslater;

        //if we have picked a destination
        ITypeTranslater destinationTypeTranslater;
        destinationTypeTranslater = _planManager.TargetDatabase != null
            ? _planManager.TargetDatabase.Server.GetQuerySyntaxHelper().TypeTranslater
            : //ensure we handle type translation between the two platforms
            sourceTypeTranslater; //otherwise (we haven't picked a destination yet)

        return Plan switch
        {
            Plan.Drop => null,
            Plan.ANO => ANOTable == null
                ? "Unknown"
                : sourceTypeTranslater.TranslateSQLDBType(ANOTable.GetRuntimeDataType(LoadStage.PostLoad),
                    destinationTypeTranslater),
            Plan.Dilute => Dilution == null
                ? "Unknown"
                : destinationTypeTranslater.GetSQLDBTypeForCSharpType(Dilution.ExpectedDestinationType),
            Plan.PassThroughUnchanged =>
                //if they have an identity column then we substitute it for int in the destination
                ColumnInfo.IsAutoIncrement
                    ? destinationTypeTranslater.GetSQLDBTypeForCSharpType(new DatabaseTypeRequest(typeof(int)))
                    : sourceTypeTranslater.TranslateSQLDBType(ColumnInfo.Data_type, destinationTypeTranslater),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public enum Plan
{
    Drop,
    ANO,
    Dilute,
    PassThroughUnchanged
}