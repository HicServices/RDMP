// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Naming;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Records the last known state of a column in an SQL table.  Can be synchronized using <see cref="TableInfoSynchronizer"/>.
/// 
/// <para>A ColumnInfo can belong to an anonymisation group (ANOTable) e.g. ANOGPCode, in this case it will be aware not only of its name and datatype
/// in LIVE but also its unanonymised name/datatype (see method GetRuntimeName(LoadStage stage)).</para>
/// 
/// <para>ColumnInfo ensures a cached representation of the underlying database so that RDMP can rationalize and inform the system user of disappearing
/// columns etc and let the user make decisions about how to resolve it.</para>
/// </summary>
public class ColumnInfo : DatabaseEntity, IComparable, IResolveDuplication, IHasDependencies, ICheckable,
    IHasQuerySyntaxHelper, IHasFullyQualifiedNameToo, ISupplementalColumnInformation, IInjectKnown<TableInfo>, INamed
{
    #region Database Properties

    private int _tableInfoID;
    private int? _anoTableID;
    private string _name;
    private string _dataType;
    private string _format;
    private string _digitisationSpecs;
    private string _source;
    private string _description;
    private ColumnStatus? _status;
    private string _regexPattern;
    private string _validationRules;
    private bool _isPrimaryKey;
    private bool _isAutoIncrement;
    private int? _duplicateRecordResolutionOrder;
    private bool _duplicateRecordResolutionIsAscending;
    private string _collation;
    private bool _ignoreInLoads;

    /// <summary>
    /// ID of the <see cref="TableInfo"/> that this <see cref="ColumnInfo"/> belongs to.
    /// </summary>
    public int TableInfo_ID
    {
        get => _tableInfoID;
        private set => SetField(ref _tableInfoID, value);
    }

    /// <summary>
    /// ID of the <see cref="ANOTable"/> transform that is applied to this <see cref="ColumnInfo"/> during
    /// data loads e.g. swap chi for anochi.
    /// </summary>
    public int? ANOTable_ID
    {
        get => _anoTableID;
        set => SetField(ref _anoTableID, value);
    }

    /// <summary>
    /// The fully specified name of the column in the underlying database table this record points at.
    /// </summary>
    [Sql]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc/>
    public string Data_type
    {
        get => _dataType;
        set => SetField(ref _dataType, value);
    }

    /// <summary>
    ///  User specified free text field.  Not used for anything by RDMP.
    /// <para> Use <see cref="Collation"/> instead</para>
    /// </summary>
    public string Format
    {
        get => _format;
        set => SetField(ref _format, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Digitisation_specs
    {
        get => _digitisationSpecs;
        set => SetField(ref _digitisationSpecs, value);
    }

    /// <summary>
    ///  User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Source
    {
        get => _source;
        set => SetField(ref _source, value);
    }

    /// <summary>
    ///  User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>
    ///  User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public ColumnStatus? Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    /// <summary>
    /// Validation Regex that could be used to assess the cleanliness of data in the column.   Not used for anything by RDMP.
    /// <para>Use the data quality engine instead (See <see cref="Catalogue.ValidatorXML"/>)</para>
    /// </summary>
    public string RegexPattern
    {
        get => _regexPattern;
        set => SetField(ref _regexPattern, value);
    }

    /// <summary>
    /// Not used for anything by RDMP.
    /// <para>Use the data quality engine instead (See <see cref="Catalogue.ValidatorXML"/>)</para>
    /// </summary>
    public string ValidationRules
    {
        get => _validationRules;
        set => SetField(ref _validationRules, value);
    }

    /// <inheritdoc/>
    public bool IsPrimaryKey
    {
        get => _isPrimaryKey;
        set => SetField(ref _isPrimaryKey, value);
    }

    /// <inheritdoc/>
    public bool IsAutoIncrement
    {
        get => _isAutoIncrement;
        set => SetField(ref _isAutoIncrement, value);
    }

    /// <inheritdoc/>
    public string Collation
    {
        get => _collation;
        set => SetField(ref _collation, value);
    }

    /// <summary>
    /// The importance of this column in resolving primary key collisions during data loads (in RAW).  Columns with a lower number are consulted first when resolving
    /// collisions.  E.g. are the colliding records different on this column? if yes use <see cref="DuplicateRecordResolutionIsAscending"/> to pick which to delete
    /// otherwise move onto the next (non primary key) column.
    /// <para>Only applies if you have a PrimaryKeyCollisionResolverMutilation in the data load</para>
    /// </summary>
    public int? DuplicateRecordResolutionOrder
    {
        get => _duplicateRecordResolutionOrder;
        set => SetField(ref _duplicateRecordResolutionOrder, value);
    }

    /// <summary>
    /// Used in primary key collision resolution during data loads (in RAW).  If two records differ on this field (and <see cref="IsPrimaryKey"/> is false) then the order
    /// (<see cref="DuplicateRecordResolutionIsAscending"/>) will be used to decide which is deleted.
    /// <para>Only applies if you have a PrimaryKeyCollisionResolverMutilation in the data load</para>
    /// </summary>
    public bool DuplicateRecordResolutionIsAscending
    {
        get => _duplicateRecordResolutionIsAscending;
        set => SetField(ref _duplicateRecordResolutionIsAscending, value);
    }

    /// <summary>
    /// Set to True to ignore this column when doing data loads
    /// </summary>
    public bool IgnoreInLoads
    {
        get => _ignoreInLoads;
        set => SetField(ref _ignoreInLoads, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="TableInfo_ID"/>
    [NoMappingToDatabase]
    public TableInfo TableInfo => _knownTableInfo.Value;

    /// <inheritdoc cref="ANOTable_ID"/>
    [NoMappingToDatabase]
    public ANOTable ANOTable => ANOTable_ID == null ? null : Repository.GetObjectByID<ANOTable>((int)ANOTable_ID);

    /// <summary>
    /// Fetches all <see cref="ExtractionInformation"/> which draw on this <see cref="ColumnInfo"/>.  This could be none (if it is not extractable) or more than one
    /// (if there are multiple extraction transforms available for the column or if the column/table is part of multiple <see cref="Catalogue"/>)
    /// </summary>
    [NoMappingToDatabase]
    public IEnumerable<ExtractionInformation> ExtractionInformations
    {
        get { return CatalogueItems.Select(e => e.ExtractionInformation).Where(o => o != null); }
    }

    /// <summary>
    /// Fetches all <see cref="CatalogueItem"/> which describe the <see cref="ExtractionInformations"/> of this <see cref="ColumnInfo"/>.  This will also include any
    /// non extractable <see cref="CatalogueItem"/> linked to this <see cref="ColumnInfo"/> in <see cref="Catalogue"/>s.
    /// </summary>
    [NoMappingToDatabase]
    public IEnumerable<CatalogueItem> CatalogueItems => Repository.GetAllObjectsWithParent<CatalogueItem>(this);

    #endregion

    /// <summary>
    /// Notional usage status of a <see cref="ColumnInfo"/>.  Not used for anything by RDMP.
    /// </summary>
    public enum ColumnStatus
    {
        /// <summary>
        /// Should not be used anymore.  Not used for anything by RDMP.
        /// </summary>
        Deprecated,

        /// <summary>
        /// Notional usage state of a <see cref="ColumnInfo"/>.  Not used for anything by RDMP.
        /// </summary>
        Inactive,

        /// <summary>
        /// Notional usage state of a <see cref="ColumnInfo"/>.  Not used for anything by RDMP.
        /// </summary>
        Archived,

        /// <summary>
        /// Normal state for columns.  Not used for anything by RDMP.
        /// </summary>
        Active
    }

    public ColumnInfo()
    {
        ClearAllInjections();
    }

    /// <summary>
    /// Creates a new record of a column found on a database server in the table referenced by <see cref="TableInfo"/>.  This constructor will be used
    /// when first importing a table reference (See <see cref="TableInfoImporter"/>)  and again whenever there are new columns
    /// discovered during table sync (See <see cref="TableInfoSynchronizer"/>)
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="parent"></param>
    public ColumnInfo(ICatalogueRepository repository, string name, string type, ITableInfo parent)
    {
        //defaults
        DuplicateRecordResolutionIsAscending = true;

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name != null ? (object)name : DBNull.Value },
            { "Data_type", type != null ? (object)type : DBNull.Value },
            { "TableInfo_ID", parent.ID },
            { "IgnoreInLoads", false }
        });

        ClearAllInjections();
    }

    private int? _datasetID;
    /// <summary>
    /// The ID of the dataset this column information came from
    /// </summary>
    [DoNotExtractProperty]
    public int? Dataset_ID
    {
        get => _datasetID;
        set => SetField(ref _datasetID, value);
    }


    internal ColumnInfo(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        TableInfo_ID = int.Parse(r["TableInfo_ID"].ToString());
        Name = r["Name"].ToString();
        Data_type = r["Data_type"].ToString();
        Format = r["Format"].ToString();
        Digitisation_specs = r["Digitisation_specs"].ToString();
        Source = r["Source"].ToString();
        Description = r["Description"].ToString();
        Collation = r["Collation"] as string;
        IgnoreInLoads = ObjectToNullableBool(r["IgnoreInLoads"]) ?? false;
        if (r["Dataset_ID"] != DBNull.Value)
            Dataset_ID = int.Parse(r["Dataset_ID"].ToString());

        //try to turn string value in database into enum value
        if (Enum.TryParse(r["Status"].ToString(), out ColumnStatus dbStatus))
            Status = dbStatus;

        RegexPattern = r["RegexPattern"].ToString();
        ValidationRules = r["ValidationRules"].ToString();
        IsPrimaryKey = bool.Parse(r["IsPrimaryKey"].ToString());
        IsAutoIncrement = bool.Parse(r["IsAutoIncrement"].ToString());

        if (r["ANOTable_ID"] != DBNull.Value)
            ANOTable_ID = int.Parse(r["ANOTable_ID"].ToString());
        else
            ANOTable_ID = null;

        if (r["DuplicateRecordResolutionOrder"] != DBNull.Value)
            DuplicateRecordResolutionOrder = int.Parse(r["DuplicateRecordResolutionOrder"].ToString());
        else
            DuplicateRecordResolutionOrder = null;

        DuplicateRecordResolutionIsAscending = Convert.ToBoolean(r["DuplicateRecordResolutionIsAscending"]);

        ClearAllInjections();
    }

    /// <summary>
    /// Returns the fully qualified <see cref="Name"/> of the <see cref="ColumnInfo"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Name;

    /// <summary>
    /// Allows sorting by fully qualified <see cref="Name"/>.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int CompareTo(object obj)
    {
        if (obj is ColumnInfo)
            return -string.Compare(obj.ToString(), ToString(),
                StringComparison.CurrentCulture); //sort alphabetically (reverse)

        throw new Exception($"Cannot compare {GetType().Name} to {obj.GetType().Name}");
    }
    public override bool Equals(object obj)
    {
        return CompareTo(obj) == 1;
    }

    ///<inheritdoc/>
    public string GetRuntimeName() => Name == null ? null : GetQuerySyntaxHelper().GetRuntimeName(Name);

    ///<inheritdoc/>
    public string GetFullyQualifiedName() => Name;

    private IQuerySyntaxHelper _cachedQuerySyntaxHelper;
    private Lazy<TableInfo> _knownTableInfo;

    ///<inheritdoc/>
    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        return _cachedQuerySyntaxHelper ??= TableInfo.GetQuerySyntaxHelper();
    }

    ///<inheritdoc/>
    public string GetRuntimeName(LoadStage stage)
    {
        var finalName = GetRuntimeName();

        if (stage <= LoadStage.AdjustRaw)
            //see if it has an ANO Transform on it
            if (ANOTable_ID != null && finalName.StartsWith("ANO"))
                return finalName["ANO".Length..];

        //any other stage will be the regular final name
        return finalName;
    }

    /// <summary>
    /// Gets the DataType adjusted for the stage at which the ColumnInfo is at, this is almost always the same as Data_type.  The only
    /// time it is different is when there is an ANOTable involved e.g. ANOLocation could be a varchar(6) like 'AB10_L' after anonymisation
    /// but if the LoadStage is AdjustRaw then it would have a value like 'NH10' (varchar(4) - the unanonymised state).
    /// </summary>
    /// <param name="loadStage"></param>
    /// <returns></returns>
    public string GetRuntimeDataType(LoadStage loadStage)
    {
        if (loadStage <= LoadStage.AdjustRaw)
        {
            //if it has an ANO transform
            if (ANOTable_ID != null)
                return
                    ANOTable.GetRuntimeDataType(
                        loadStage); //get the datatype from the ANOTable because ColumnInfo is of mutable type depending on whether it has been anonymised yet

            //it doesn't have an ANOtransform but it might be the subject of dilution
            var discard = TableInfo.PreLoadDiscardedColumns.SingleOrDefault(c =>
                c.GetRuntimeName().Equals(GetRuntimeName(), StringComparison.InvariantCultureIgnoreCase));

            //The column exists both in the live database and in the identifier dump.  This is because it goes through horrendous bitcrushing operations e.g. Load RAW with full
            //postcode varchar(8) and ship postcode off to identifier dump but also let it go through to live but only as the first 4 letters varchar(4).  so the datatype of the column
            //in RAW is varchar(8) but in Live is varchar(4)
            return discard != null ? discard.Data_type : Data_type;
        }

        //The user is asking about a stage other than RAW so tell them about the final column type state
        return Data_type;
    }

    /// <summary>
    /// Connects to the live database referenced by this <seealso cref="ColumnInfo"/> and discovers the column returning the
    /// live state of the column.
    /// <para>If the database/table/column doesn't exist or is inaccessible then this method will throw</para>
    /// </summary>
    /// <param name="context">Determines which credentials (if any) to use to perform the connection operation</param>
    /// <returns>The live state of the column</returns>
    public DiscoveredColumn Discover(DataAccessContext context)
    {
        var ti = TableInfo;
        var db = DataAccessPortal.ExpectDatabase(ti, context);
        return db.ExpectTable(ti.GetRuntimeName()).DiscoverColumn(GetRuntimeName());
    }

    ///<inheritdoc/>
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        var iDependOn = new List<IHasDependencies>();

        iDependOn.AddRange(CatalogueItems);
        iDependOn.Add(TableInfo);

        if (ANOTable_ID != null)
            iDependOn.Add(ANOTable);

        return iDependOn.ToArray();
    }


    ///<inheritdoc/>
    public IHasDependencies[] GetObjectsDependingOnThis()
    {
        var dependantObjects = new List<IHasDependencies>();

        //also any CatalogueItems that reference us
        dependantObjects.AddRange(CatalogueItems);

        //also lookups are dependent on us
        dependantObjects.AddRange(GetAllLookupForColumnInfoWhereItIsA(LookupType.AnyKey));

        //also join infoslookups are dependent on us
        dependantObjects.AddRange(
            Repository.GetAllObjects<JoinInfo>().Where(j =>
                j.ForeignKey_ID == ID ||
                j.PrimaryKey_ID == ID));

        return dependantObjects.ToArray(); //dependantObjects.ToArray();
    }

    /// <summary>
    /// Checks the ANO status of the column is valid (and matching on datatypes etc).
    /// <para>Does not check for synchronization against the underlying database.</para>
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        //if it does not have an ANO transform it should not start with ANO
        if (ANOTable_ID == null)
        {
            //the column has no anotable
            //make sure it doesn't start with ANO (if it does it should have an ANOTable, maybe the user is calling his column ANOUNCEMENT or something (not permitted)
            if (GetRuntimeName().StartsWith(ANOTable.ANOPrefix))
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"ColumnInfo {this} (ID={ID}) begins with {ANOTable.ANOPrefix} but does not have an ANOTable configured for it",
                    CheckResult.Warning, null));
        }
        else //if it does have an ANO transform it must start with ANO
        if (!GetRuntimeName().StartsWith(ANOTable.ANOPrefix))
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"ColumnInfo {this} (ID={ID}) has an ANOTable configured but does not start with {ANOTable.ANOPrefix} (All anonymised columns must start with {ANOTable.ANOPrefix})",
                CheckResult.Fail, null));
        }
    }

    /// <summary>
    /// Determines whether the <see cref="ColumnInfo"/> is involved in <see cref="Lookup"/> declarations (either as a foreign key, or as part of a lookup <see cref="TableInfo"/>).
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Lookup[] GetAllLookupForColumnInfoWhereItIsA(LookupType type)
    {
        if (type == LookupType.Description)
            return Repository.GetAllObjectsWhere<Lookup>("Description_ID", ID);
        if (type == LookupType.AnyKey)
            return Repository.GetAllObjectsWhere<Lookup>("ForeignKey_ID", ID, ExpressionType.OrElse, "PrimaryKey_ID",
                ID);
        return type == LookupType.ForeignKey
            ? Repository.GetAllObjectsWhere<Lookup>("ForeignKey_ID", ID)
            : throw new NotImplementedException($"Unrecognised LookupType {type}");
    }

    ///<inheritdoc/>
    public void InjectKnown(TableInfo instance)
    {
        _knownTableInfo = new Lazy<TableInfo>(instance);
    }

    ///<inheritdoc/>
    public void ClearAllInjections()
    {
        _knownTableInfo = new Lazy<TableInfo>(() => Repository.GetObjectByID<TableInfo>(TableInfo_ID));
    }

    /// <summary>
    /// Returns true if the Data_type is numerical (decimal or int) according to the DBMS it resides in.  Returns
    /// false if the the Data_type is not found to be numerical or if the datatype is unknown, missing or anything
    /// else goes wrong resolving the Type.
    /// </summary>
    /// <returns></returns>
    public bool IsNumerical()
    {
        try
        {
            //is it numerical?
            var cSharpType = GetQuerySyntaxHelper().TypeTranslater.GetCSharpTypeForSQLDBType(Data_type);
            return cSharpType == typeof(decimal) || cSharpType == typeof(int);
        }
        catch (Exception)
        {
            return false;
        }
    }
}