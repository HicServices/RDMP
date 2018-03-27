using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ANOStore.ANOEngineering
{
    /// <summary>
    /// Records all ANO descisions made about a single ColumnInfo (e.g. whether to ANO it, Drop it, Dillute it etc)
    /// </summary>
    public class ColumnInfoANOPlan:ICheckable
    {
        private readonly ColumnInfo _columnInfo;
        private readonly ExtractionInformation[] _allExtractionInformations;
        private readonly CatalogueItem[] _allCatalogueItems;
        private readonly List<JoinInfo> _joins;
        private readonly List<Lookup> _lookups;
        private readonly ColumnInfo[] _allColumnInfosSystemWide;
        private IQuerySyntaxHelper _querySyntaxHelper;
        private readonly ForwardEngineerANOCataloguePlanManager _planManager;
        private Plan _plan;

        public ANOTable ANOTable {get;set;}
        public IDilutionOperation Dilution { get; set; }
        public ExtractionCategory? ExtractionCategoryIfAny { get; set; }

        public Plan Plan
        {
            get { return _plan; }
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
                        throw new ArgumentOutOfRangeException("value");
                }
            }
        }

        public bool IsMandatory
        {
            get
            {
                //ColumnInfo is part of the table primary key
                return _columnInfo.IsPrimaryKey;
            }
        }

        /// <summary>
        /// Make up ANOTable plans based on existing ANOTable/column usages.  For example if the column chi is being migrated and there is at least one column
        /// called chi or ANOchi already existing (probably from another table) then we should suggest using ANOTable ANOchi.
        /// </summary>
        /// <returns></returns>
        public ColumnInfoANOPlan(ColumnInfo columnInfo, ExtractionInformation[] allExtractionInformations, CatalogueItem[] allCatalogueItems, List<JoinInfo> joins, List<Lookup> lookups, ColumnInfo[] allColumnInfosSystemWide, IQuerySyntaxHelper querySyntaxHelper,ForwardEngineerANOCataloguePlanManager planManager)
        {
            _columnInfo = columnInfo;
            _allExtractionInformations = allExtractionInformations;
            _allCatalogueItems = allCatalogueItems;
            _joins = joins;
            _lookups = lookups;
            _allColumnInfosSystemWide = allColumnInfosSystemWide;
            _querySyntaxHelper = querySyntaxHelper;
            _planManager = planManager;

            //get an extraction category based on it's current extractability
            ExtractionCategoryIfAny = GetMaxExtractionCategoryIfAny();

            if (_columnInfo.GetRuntimeName().StartsWith("hic_"))
                Plan = Plan.Drop;//suggest dropping hic_ fields
            else
            if(_columnInfo.IsPrimaryKey || IsInvolvedInLookups() || IsInvolvedInJoins())
                Plan = Plan.PassThroughUnchanged; //if it's involved in lookups, joins or is a primary key
            else
            if(ExtractionCategoryIfAny == null) //if it isn't extractable
                Plan = Plan.Drop;
            else
                Plan = Plan.PassThroughUnchanged; //it is extractable but not special

            //if theres an associated ANOTable with a different ColumnInfo with the same name e.g. chi=>ANOChi in another dataset that was already anonymised
            MakeANOTableSuggestionIfApplicable();
        }


        private ExtractionCategory? GetMaxExtractionCategoryIfAny()
        {
            ExtractionCategory? toReturn = null;

            //Decide on an ExtractionCategory
            //setup the planned extraction category based on what CatalogueItem(s) exist for it (1 to many)
            foreach (var catalogueItem in _allCatalogueItems.Where(ci => ci.ColumnInfo_ID == _columnInfo.ID).ToArray())
            {
                //that are extractable
                var extractionInformation = _allExtractionInformations.SingleOrDefault(ei => ei.CatalogueItem_ID == catalogueItem.ID);

                //The ColumnInfo is extractable
                if (extractionInformation != null)
                {
                    if (toReturn == null)
                        toReturn = extractionInformation.ExtractionCategory;
                    
                    //there are multiple, if the new one is more restrictive then use the more restrictive category instead
                    toReturn = extractionInformation.ExtractionCategory > toReturn ? extractionInformation.ExtractionCategory : toReturn;
                }
            }

            return toReturn;
        }

        private void MakeANOTableSuggestionIfApplicable()
        {
            //if there is a ColumnInfo with the same name (or that has ANO prefix)
            var matchingOnName = _allColumnInfosSystemWide.Where(a => a.GetRuntimeName() == _columnInfo.GetRuntimeName() || a.GetRuntimeName() == "ANO" + _columnInfo.GetRuntimeName()).ToArray();

            //and if the same named ColumnInfo(s) have a shared ANOTable (e.g. ANOCHI)
            var agreedAnoTableID = matchingOnName.Where(c => c.ANOTable_ID != null).Select(c => c.ANOTable_ID).Distinct().ToArray();

            //if there is a single recommended anotable id amongst all columns with matching name featuring ano tables 
            if (agreedAnoTableID.Count() == 1)
            {
                ANOTable = _columnInfo.Repository.GetObjectByID<ANOTable>(agreedAnoTableID.Single().Value);
                Plan = Plan.ANO;
            }
        }

        public bool IsInvolvedInLookups()
        {
            return _lookups.Any(l=>l.Description_ID == _columnInfo.ID || l.ForeignKey_ID == _columnInfo.ID || l.PrimaryKey_ID == l.ID);
        }
        private bool IsInvolvedInJoins()
        {
            return _joins.Any(j => j.PrimaryKey_ID == _columnInfo.ID || j.ForeignKey_ID == _columnInfo.ID);
        }

        public void Check(ICheckNotifier notifier)
        {
            if(IsMandatory && Plan == Plan.Drop)
                notifier.OnCheckPerformed(new CheckEventArgs("ColumnInfo '" + _columnInfo + "' is mandatory and cannot be dropped",CheckResult.Fail));

            if(Plan == Plan.ANO && ANOTable == null)
                notifier.OnCheckPerformed(new CheckEventArgs("No ANOTable has been picked for ColumnInfo '" + _columnInfo + "'", CheckResult.Fail));

            if(Plan == Plan.Dilute && Dilution == null)
                notifier.OnCheckPerformed(new CheckEventArgs("No Dilution Operation has been picked for ColumnInfo '" + _columnInfo + "'", CheckResult.Fail));
            
            if (Plan != Plan.Drop)
            {
                try
                {
                    var datatype = GetEndpointDataType();
                    notifier.OnCheckPerformed(new CheckEventArgs("Determined endpoint data type '" + datatype + "' for ColumnInfo '" + _columnInfo + "'", CheckResult.Success));

                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not determine endpoint data type for ColumnInfo '" + _columnInfo + "'", CheckResult.Fail, e));
                }
            }
            
            

            //don't let user select ExtractionCategory.Any
            if (ExtractionCategoryIfAny == ExtractionCategory.Any)
                notifier.OnCheckPerformed(new CheckEventArgs("Extraction Category '" + ExtractionCategoryIfAny + "' is not valid (on ColumnInfo " + _columnInfo + ")", CheckResult.Fail));

            //for each column we are not planning on dropping but are planning on making extractable, there must be a CatalogueItem in the source Catalogue
            if (ExtractionCategoryIfAny != null && Plan != Plan.Drop)
                if (_allCatalogueItems.All(ci => ci.ColumnInfo_ID != _columnInfo.ID))
                    notifier.OnCheckPerformed(
                        new CheckEventArgs("There are no CatalogueItems configured for ColumnInfo '" + _columnInfo + "' but it's PlannedExtractionCategory is '" + ExtractionCategoryIfAny + "'", CheckResult.Fail));

            //Will there be conflicts on name?
            if (_planManager.TargetDatabase != null && Plan != Plan.Drop)
            {
                //don't let them extract to the same database
                if(_planManager.TargetDatabase.GetRuntimeName() == _columnInfo.TableInfo.Database)
                    notifier.OnCheckPerformed(new CheckEventArgs("ColumnInfo " + _columnInfo + " is already in " + _planManager.TargetDatabase.GetRuntimeName() +" you cannot create an ANO version in the same database",CheckResult.Fail));
            }
        }

        public string GetEndpointDataType()
        {
            var sourceTypeTranslater = _querySyntaxHelper.TypeTranslater;

            //if we have picked a destination
            ITypeTranslater destinationTypeTranslater;
            if (_planManager.TargetDatabase != null)
                destinationTypeTranslater = _planManager.TargetDatabase.Server.GetQuerySyntaxHelper().TypeTranslater;//ensure we handle type translation between the two platforms
            else
                destinationTypeTranslater = sourceTypeTranslater;//otherwise (we haven't picked a destination yet)

            switch (Plan)
            {
                case Plan.Drop:
                    return null;
                case Plan.ANO:
                    if (ANOTable == null)
                        return "Unknown";

                    return sourceTypeTranslater.TranslateSQLDBType(ANOTable.GetRuntimeDataType(LoadStage.PostLoad), destinationTypeTranslater);
                case Plan.Dilute:
                    if (Dilution == null)
                        return "Unknown";

                    return destinationTypeTranslater.GetSQLDBTypeForCSharpType(Dilution.ExpectedDestinationType);

                case Plan.PassThroughUnchanged:

                    return sourceTypeTranslater.TranslateSQLDBType(_columnInfo.Data_type, destinationTypeTranslater);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum Plan
    {
        Drop,
        ANO,
        Dilute,
        PassThroughUnchanged
    }

}
