// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.Curation.Data.Aggregation;

/// <summary>
/// Entry point for the aggregation system.  This class describes what a given aggregation is supposed to achieve (e.g. summarise the number of records in a
/// dataset by region over time since 2001 to present).  An AggregateConfiguration belongs to a given Catalogue and is the hanging-off point for the rest of
/// the configuration (e.g. AggregateDimension / AggregateFilter)
///
/// <para>AggregateConfigurations can be used with an AggregateBuilder to produce runnable SQL which will return a DataTable containing results appropriate to the
/// query being built.</para>
///
/// <para>There are Three types of AggregateConfiguration (these are configurations - not separate classes):</para>
/// <para>1. 'Aggregate Graph' - Produce summary information about a dataset designed to be displayed in a graph e.g. number of records each year by healthboard</para>
/// <para>2. 'Cohort Aggregate' - Produce a list of unique patient identifiers from a dataset (e.g. 'all patients with HBA1c test code > 50 in biochemistry')</para>
/// <para>3. 'Joinable PatientIndex Table' - Produce a patient identifier fact table for joining to other Cohort Aggregates during cohort building (See JoinableCohortAggregateConfiguration)</para>
/// <para>The above labels are informal terms.  Use IsCohortIdentificationAggregate and IsJoinablePatientIndexTable to determine what type a given
/// AggregateConfiguration is. </para>
///
/// <para>If your Aggregate is part of cohort identification (Identifier List or Patient Index Table) then its name will start with cic_X_ where X is the ID of the cohort identification
/// configuration.  Depending on the user interface though this might not appear (See ToString implementation).</para>
/// </summary>
public class AggregateConfiguration : DatabaseEntity, ICheckable, IOrderable, ICollectSqlParameters, INamed,
    IHasDependencies, IHasQuerySyntaxHelper,
    IInjectKnown<JoinableCohortAggregateConfiguration>,
    IInjectKnown<AggregateDimension[]>,
    IInjectKnown<Catalogue>,
    IDisableable, IKnowWhatIAm, IRootFilterContainerHost
{
    #region Database Properties

    private string _countSQL;
    private int _catalogueID;
    private string _name;
    private string _description;
    private DateTime _dtCreated;
    private int? _pivotOnDimensionID;
    private bool _isExtractable;
    private string _havingSQL;
    private bool _isDisabled;


    /// <summary>
    /// The count(*) or sum(*) or count(distinct chi) etc column of an AggregateConfiguration group by
    /// </summary>
    public string CountSQL
    {
        get => _countSQL;
        set => SetField(ref _countSQL, value);
    }

    /// <summary>
    /// The ID of the Catalogue (dataset) that the AggregateConfiguration belongs to.  This determines which tables/server it will be run on in addition to what filters/columns are
    /// importable etc.
    /// </summary>
    public int Catalogue_ID
    {
        get => _catalogueID;
        set => SetField(ref _catalogueID, value);
    }

    /// <summary>
    /// The unique name of the aggregate e.g. 'Biochemistry records by year divided by healthboard'
    /// </summary>
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// A human readable description of what the AggregateConfiguration is trying to depict or represent
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>
    /// Automatically populated field indicating when the AggregateConfiguration was created in the database (you really shouldn't change this field)
    /// </summary>
    public DateTime dtCreated
    {
        get => _dtCreated;
        set => SetField(ref _dtCreated, value);
    }

    /// <summary>
    /// Indicates the AggregateDimension (if any) that will result in a pivot graph being generated.  E.g. if your AggregateConfiguration is a graph of records by year between
    /// 2001 and 2018 then specifying a pivot on healthboard would result in 1 line in the graph per healthboard instead of a single line for the count of all (the default).
    ///
    /// <para>If an AggregateConfiguration is a Cohort or Patient index table then it cannot have a Pivot</para>
    /// </summary>
    public int? PivotOnDimensionID
    {
        get => _pivotOnDimensionID;
        set => SetField(ref _pivotOnDimensionID, value);
    }

    /// <summary>
    /// Flag that indicates whether an AggregateConfiguration which is functioning as a graph can be exposed to users without worrying about governance.  This manifests as whether
    /// you can use the aggregate graph to supply information about an extraction etc.
    /// </summary>
    public bool IsExtractable
    {
        get => _isExtractable;
        set => SetField(ref _isExtractable, value);
    }


    /// <summary>
    /// Specifies that HAVING section of the GROUP BY statement represented by this AggregateConfiguration.  For example you could specify patients on drug X HAVING count(*) > 2 to
    /// indicate that they must have had 2+ of the drug (ever or in the month being looked at if it is a graph).  This can have unexpected consequences if you have a pivot and axis
    /// etc since the having will apply only to the specific bucket (date section and pivot value) being evaluated at each step.
    /// </summary>
    public string HavingSQL
    {
        get => _havingSQL;
        set => SetField(ref _havingSQL, value);
    }

    /// <summary>
    /// ID of the AND/OR container of filters (which might be empty) that will restrict the records matched by the AggregateConfiguration GROUP by.  All filters/containers will
    /// be processed recursively and built up into appropriate WHERE sql at query building time.
    /// </summary>
    public int? RootFilterContainer_ID
    {
        get => _rootFilterContainerID;
        set
        {
            if (OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null && value != null)
                throw new NotSupportedException(
                    $"This AggregateConfiguration has a shortcut to another AggregateConfiguration's Filters (its OverrideFiltersByUsingParentAggregateConfigurationInstead_ID is {OverrideFiltersByUsingParentAggregateConfigurationInstead_ID}) which means it cannot be assigned its own RootFilterContainerID");

            SetField(ref _rootFilterContainerID, value);
        }
    }

    /// <summary>
    /// Specify instead of RootFilterContainer_ID to indicate that this AggregateConfiguration should instead use the filters of a different AggregateConfiguration.  This is
    /// generally only useful if you have an AggregateConfiguration which you are using in cohort generation (e.g. prescriptions for drug x) and you want to generate another
    /// AggregateConfiguration which is a graph of those results by year and you don't want to duplicate the filter configuration.
    /// </summary>
    public int? OverrideFiltersByUsingParentAggregateConfigurationInstead_ID
    {
        get => _overrideFiltersByUsingParentAggregateConfigurationInsteadID;
        set
        {
            if (RootFilterContainer_ID != null && value != null)
                throw new NotSupportedException(
                    "Cannot set OverrideFiltersByUsingParentAggregateConfigurationInstead_ID because this AggregateConfiguration already has a filter container set (if you were to be a shortcut and also have a filter tree set it would be very confusing)");

            SetField(ref _overrideFiltersByUsingParentAggregateConfigurationInsteadID, value);
        }
    }

    /// <inheritdoc/>
    public bool IsDisabled
    {
        get => _isDisabled;
        set => SetField(ref _isDisabled, value);
    }

    #endregion

    #region Relationships

    /// <summary>
    /// Fetches the Catalogue referenced by Catalogue_ID
    /// <see cref="Catalogue_ID"/>
    ///  </summary>
    [NoMappingToDatabase]
    public Catalogue Catalogue => _knownCatalogue.Value;

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public IContainer RootFilterContainer
    {
        get
        {
            //if there is an override
            if (OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null)
                return Repository.GetObjectByID<AggregateConfiguration>(
                    (int)OverrideFiltersByUsingParentAggregateConfigurationInstead_ID).RootFilterContainer;
            //return the overriding one

            //else return the actual root filter container or null if there isn't one
            return RootFilterContainer_ID == null
                ? null
                : Repository.GetObjectByID<AggregateFilterContainer>((int)RootFilterContainer_ID);
        }
    }


    /// <summary>
    /// Called right before displaying an aggregate graph.  Adjust the source for that graph (<paramref name="dt"/>) e.g.
    /// discarding empty columns (based on user settings).
    /// </summary>
    /// <param name="dt"></param>
    public static void AdjustGraphDataTable(DataTable dt)
    {
        if (dt.Rows.Count == 0) return;

        if (!UserSettings.IncludeZeroSeriesInGraphs)
        {
            dt.BeginLoadData();
            foreach (var col in dt.Columns.Cast<DataColumn>().ToArray())
                if (dt.Rows.Cast<DataRow>().All(r => IsBasicallyZero(r[col.ColumnName])))
                    dt.Columns.Remove(col);
            dt.EndLoadData();
        }
    }

    private static bool IsBasicallyZero(object v) => v == null || v == DBNull.Value ||
                                                     string.IsNullOrWhiteSpace(v.ToString()) ||
                                                     string.Equals(v.ToString(), "0");

    /// <summary>
    /// Gets all parameters (e.g. @studyStartDate ) associated with this AggregateConfiguration (there might not be any).
    /// </summary>
    [NoMappingToDatabase]
    public IEnumerable<AnyTableSqlParameter> Parameters => CatalogueRepository.GetAllParametersForParentTable(this);

    /// <inheritdoc cref="Parameters"/>
    public ISqlParameter[] GetAllParameters() => Parameters.ToArray();

    /// <summary>
    /// An AggregateConfiguration is a Group By statement.  This will involve using at least one Table in the FROM section of the query.  The descision on which tables
    /// to join is made by the AggregateBuilder based on the AggregateDimensions (columns).  If there are no column mapped AggregateDimensions (e.g. there is only a count(*))
    /// or there are other tables you want joined in addition the user can specify them in this property Populated via <see cref="AggregateForcedJoin"/>
    /// </summary>
    [NoMappingToDatabase]
    public ITableInfo[] ForcedJoins => CatalogueRepository.AggregateForcedJoinManager.GetAllForcedJoinsFor(this);

    /// <summary>
    /// When an AggregateConfiguration is used in a cohort identification capacity it can have one or more 'patient index tables' defined e.g.
    /// 'Give me all prescriptions for morphine' (Prescribing) 'within 6 months of patient being discharged from hospital' (SMR01).  In this case
    /// a join is done against the secondary dataset.
    ///
    /// <para>This property returns all such 'patient index table' AggregateConfigurations which are currently being used by this AggregateConfiguration
    /// for building its join.</para>
    /// </summary>
    [NoMappingToDatabase]
    public JoinableCohortAggregateConfigurationUse[] PatientIndexJoinablesUsed => Repository
        .GetAllObjectsWithParent<JoinableCohortAggregateConfigurationUse>(this).ToArray();

    /// <summary>
    /// Only populated if the AggregateConfiguration is acting as a patient index table.  Returns the <see cref="JoinableCohortAggregateConfiguration"/> object
    /// which makes this a fact.
    /// </summary>
    [NoMappingToDatabase]
    public JoinableCohortAggregateConfiguration JoinableCohortAggregateConfiguration =>
        _knownJoinableCohortAggregateConfiguration.Value;

    /// <summary>
    /// An AggregateConfiguration is a Group By statement.  This will return all the SELECT columns for the query (including any count(*) / sum(*) etc columns).
    /// </summary>
    [NoMappingToDatabase]
    public AggregateDimension[] AggregateDimensions => _knownAggregateDimensions.Value;


    /// <inheritdoc cref="PivotOnDimensionID"/>
    [NoMappingToDatabase]
    public AggregateDimension PivotDimension =>
        PivotOnDimensionID == null
            ? null
            : Repository.GetObjectByID<AggregateDimension>((int)PivotOnDimensionID);

    public AggregateConfiguration()
    {
        ClearAllInjections();
    }

    /// <inheritdoc cref="OverrideFiltersByUsingParentAggregateConfigurationInstead_ID"/>
    [NoMappingToDatabase]
    public AggregateConfiguration OverrideFiltersByUsingParentAggregateConfigurationInstead =>
        _overrideFiltersByUsingParentAggregateConfigurationInsteadID == null
            ? null
            : Repository.GetObjectByID<AggregateConfiguration>(
                (int)_overrideFiltersByUsingParentAggregateConfigurationInsteadID);

    #endregion

    /// <summary>
    /// Only relevant for AggregateConfigurations that are being used in a cohort identification capacity (See <see cref="IsCohortIdentificationAggregate"/>).
    ///
    /// <para>The order location of an AggregateConfiguration within its parent <see cref="CohortAggregateContainer"/> (if it has one).  This is mostly irrelevant for UNION /
    /// INTERSECT operations (other than helping the user viewing the system) but is vital for EXCEPT containers where the first AggregateConfiguration in the container is
    /// run producing a dataset and all subsequent AggregateConfigurations are then removed from that patient set.</para>
    /// </summary>
    [NoMappingToDatabase]
    public int Order
    {
        get
        {
            if (!orderFetchAttempted)
            {
                ReFetchOrder();
                orderFetchAttempted = true;
            }

            //not within any containers
            return _orderWithinKnownContainer == null ? 0 : (int)_orderWithinKnownContainer;
        }

        set
        {
            CatalogueRepository.CohortContainerManager.SetOrder(this, value);
            _orderWithinKnownContainer = value;
        }
    }

    /// <summary>
    /// True if the AggregateConfiguration is part of a <see cref="CohortIdentificationConfiguration"/> and is intended to produce list of patient identifiers (optionally
    /// with other data if it is a 'patient index table'.
    /// </summary>
    [NoMappingToDatabase]
    public bool IsCohortIdentificationAggregate => Name.StartsWith(CohortIdentificationConfiguration.CICPrefix);

    /// <summary>
    /// Creates a new AggregateConfiguration (graph, cohort set or patient index table) in the ICatalogueRepository
    /// . database associated with the provided Catalogue (dataset).
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="catalogue"></param>
    /// <param name="name"></param>
    public AggregateConfiguration(ICatalogueRepository repository, ICatalogue catalogue, string name)
    {
        //default values
        CountSQL = "count(*)";
        dtCreated = DateTime.Now;

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "Catalogue_ID", catalogue.ID }
        });

        ClearAllInjections();
    }

    internal AggregateConfiguration(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        Name = r["Name"] as string;
        Description = r["Description"] as string;
        Catalogue_ID = int.Parse(r["Catalogue_ID"].ToString());
        dtCreated = DateTime.Parse(r["dtCreated"].ToString());

        CountSQL = r["CountSQL"] as string;
        HavingSQL = r["HavingSQL"] as string;

        var rootFilterID = r["RootFilterContainer_ID"];

        if (rootFilterID == null || rootFilterID == DBNull.Value)
            RootFilterContainer_ID = null;
        else
            RootFilterContainer_ID = int.Parse(rootFilterID.ToString());

        if (r["PivotOnDimensionID"] == DBNull.Value)
            PivotOnDimensionID = null;
        else
            PivotOnDimensionID = Convert.ToInt32(r["PivotOnDimensionID"]);

        IsExtractable = Convert.ToBoolean(r["IsExtractable"]);
        IsDisabled = Convert.ToBoolean(r["IsDisabled"]);

        OverrideFiltersByUsingParentAggregateConfigurationInstead_ID =
            ObjectToNullableInt(r["OverrideFiltersByUsingParentAggregateConfigurationInstead_ID"]);

        ClearAllInjections();
    }


    /// <summary>
    /// Updates <see cref="Order"/> from the database
    /// </summary>
    public void ReFetchOrder()
    {
        _orderWithinKnownContainer = CatalogueRepository.CohortContainerManager.GetOrderIfExistsFor(this);
    }

    private Lazy<JoinableCohortAggregateConfiguration> _knownJoinableCohortAggregateConfiguration;
    private Lazy<AggregateDimension[]> _knownAggregateDimensions;
    private Lazy<Catalogue> _knownCatalogue;

    /// <summary>
    /// All AggregateConfigurations have the potential a'Joinable Patient Index Table' (see AggregateConfiguration class documentation).  This method injects
    /// what fact that the AggregateConfiguration is definetly one by passing the JoinableCohortAggregateConfiguration that makes it one.  Pass null in to
    /// indicate that the AggregateConfiguration is definetly NOT ONE.  See also the method <see cref="IsJoinablePatientIndexTable"/>
    /// </summary>
    public void InjectKnown(JoinableCohortAggregateConfiguration instance)
    {
        _knownJoinableCohortAggregateConfiguration = new Lazy<JoinableCohortAggregateConfiguration>(instance);
    }

    /// <inheritdoc/>
    public void ClearAllInjections()
    {
        _knownJoinableCohortAggregateConfiguration = new Lazy<JoinableCohortAggregateConfiguration>(() =>
            Repository.GetAllObjectsWithParent<JoinableCohortAggregateConfiguration>(this).SingleOrDefault());
        _knownAggregateDimensions =
            new Lazy<AggregateDimension[]>(() =>
                Repository.GetAllObjectsWithParent<AggregateDimension>(this).ToArray());
        _knownCatalogue = new Lazy<Catalogue>(() => Repository.GetObjectByID<Catalogue>(Catalogue_ID));
    }

    public void InjectKnown(AggregateDimension[] instance)
    {
        _knownAggregateDimensions = new Lazy<AggregateDimension[]>(instance);
    }

    public void InjectKnown(Catalogue instance)
    {
        _knownCatalogue = new Lazy<Catalogue>(instance);
    }

    /// <summary>
    /// Returns the Name.  If the AggregateConfiguration is a cohort identification aggregate (distinguished by <see cref="CohortIdentificationConfiguration.CICPrefix"/>)
    /// then the prefix is removed from the return value.
    /// </summary>
    /// <returns></returns>
    public override string ToString() =>
        //strip the cic section from the front
        Regex.Replace(Name, $@"{CohortIdentificationConfiguration.CICPrefix}\d+_?", "");

    public bool ShouldBeReadOnly(out string reason)
    {
        var cic = GetCohortIdentificationConfigurationIfAny();
        if (cic == null)
        {
            reason = null;
            return false;
        }

        return cic.ShouldBeReadOnly(out reason);
    }

    /// <summary>
    /// Gets an IQuerySyntaxHelper from the Catalogue powering this AggregateConfiguration (<see cref="IHasQuerySyntaxHelper.GetQuerySyntaxHelper"/>)
    /// </summary>
    /// <returns></returns>
    public IQuerySyntaxHelper GetQuerySyntaxHelper() => Catalogue.GetQuerySyntaxHelper();

    /// <summary>
    /// Specifies that that given <see cref="ExtractionInformation"/> should become a new SELECT column in the GROUP BY of this AggregateConfiguration.  This
    /// column will also appear in the ORDER BY and GROUP BY sections of the query when built by <see cref="AggregateBuilder"/>.  Finally if the column comes
    /// from a novel underlying TableInfo then that new table will also be included in the FROM section of the query (e.g. with a join).
    /// </summary>
    /// <param name="basedOnColumn"></param>
    public AggregateDimension AddDimension(ExtractionInformation basedOnColumn)
    {
        ClearAllInjections();
        return new AggregateDimension((ICatalogueRepository)basedOnColumn.Repository, basedOnColumn, this);
    }

    /// <summary>
    /// Sets up a new <see cref="AggregateBuilder"/> with all the columns (See <see cref="AggregateDimensions"/>), WHERE logic (See <see cref="RootFilterContainer"/>, Pivot
    /// etc.
    /// </summary>
    /// <remarks>Note that some elements e.g. axis are automatically handles at query generation time and therefore do not have to be injected into the <see cref="AggregateBuilder"/></remarks>
    /// <param name="topX">Maximum number of rows to return, the proper way to do this is via <see cref="AggregateTopX"/></param>
    /// <returns></returns>
    public AggregateBuilder GetQueryBuilder(int? topX = null)
    {
        if (string.IsNullOrWhiteSpace(CountSQL))
            throw new NotSupportedException(
                $"Cannot generate an AggregateBuilder because the AggregateConfiguration '{this}' has no Count SQL, usually this is the case for 'Cohort Set' Aggregates or 'Patient Index Table' Aggregates.  In either case you should use CohortQueryBuilder instead of AggregateBuilder");

        var allForcedJoins = ForcedJoins.ToArray();

        var limitationSQLIfAny = topX == null ? null : $"TOP {topX.Value}";

        var builder = allForcedJoins.Any()
            ? new AggregateBuilder(limitationSQLIfAny, CountSQL, this, allForcedJoins)
            : new AggregateBuilder(limitationSQLIfAny, CountSQL, this);

        builder.AddColumnRange(AggregateDimensions.ToArray());
        builder.RootFilterContainer = RootFilterContainer;

        if (PivotOnDimensionID != null)
            builder.SetPivotToDimensionID(PivotDimension);

        return builder;
    }

    /// <summary>
    /// If an AggregateConfiguration is set up as a graph (i.e. it isn't a cohort identification set) then it can have a single <see cref="AggregateDimension"/> defined
    /// as an axis dimension (e.g. DatePrescribed).  This method returns the  <see cref="AggregateContinuousDateAxis"/> if there is one or null if there isn't one.
    /// </summary>
    /// <returns></returns>
    public AggregateContinuousDateAxis GetAxisIfAny()
    {
        //for each dimension
        foreach (var aggregateDimension in AggregateDimensions)
        {
            //if it has an axis
            var axis = aggregateDimension.AggregateContinuousDateAxis;
            if (axis != null)
                return axis; //return it
        }

        //done
        return null;
    }

    /// <summary>
    /// Returns the AggregateTopX defined for limiting the number of rows returned by the AggregateConfiguration or null if there isn't one defined.  This is only applicable
    /// on non cohort identification AggregateConfigurations
    /// </summary>
    /// <returns></returns>
    public AggregateTopX GetTopXIfAny() => Repository.GetAllObjectsWithParent<AggregateTopX>(this).SingleOrDefault();

    /// <summary>
    /// Determines whether the AggregateConfiguration could be used in a <see cref="CohortIdentificationConfiguration"/> to identify a list of patients.  This will be true
    /// if there are no pivot/axis dimensions and one of the <see cref="AggregateDimensions"/> is marked <see cref="AggregateDimension.IsExtractionIdentifier"/>
    /// </summary>
    /// <param name="reason"></param>
    /// <returns></returns>
    public bool IsAcceptableAsCohortGenerationSource(out string reason)
    {
        reason = null;

        var dimensions = AggregateDimensions;

        if (dimensions.Count(d => d.IsExtractionIdentifier) != 1)
            reason = "There must be exactly 1 Dimension which is marked IsExtractionIdentifier";

        if (PivotOnDimensionID != null)
            reason = "It cannot contain a pivot";

        if (GetAxisIfAny() != null)
            reason = "It cannot have any axes";

        return reason == null;
    }

    /// <summary>
    /// Checks that the AggregateConfiguration can be resolved into a runnable SQL quer <see cref="GetQueryBuilder"/>
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        if (HavingSQL != null && HavingSQL.StartsWith("HAVING", StringComparison.CurrentCultureIgnoreCase))
            notifier.OnCheckPerformed(new CheckEventArgs(
                "HavingSql does not need the word 'HAVING' at the start, it is implicit", CheckResult.Fail));

        //these are not checkable since they are intended for use by CohortQueryBuilder instead
        if (IsCohortIdentificationAggregate)
            return;

        //if it's a normal aggregate then don't let the user have more than 2 columns
        if (AggregateDimensions.Length > 2)
            throw new QueryBuildingException(
                "Too many columns, You can only have a maximum of 2 columns in any graph (plus a count column).  These are: \r\n 1. The time axis (if any) \r\n 2. The pivot column (if any)");

        if (AggregateDimensions.Length == 2 && !PivotOnDimensionID.HasValue)
            throw new QueryBuildingException("In order to have 2 columns, one must be selected as a pivot");

        try
        {
            var qb = GetQueryBuilder();
            notifier.OnCheckPerformed(new CheckEventArgs($"successfully generated Aggregate SQL:{qb.SQL}",
                CheckResult.Success));
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Failed to generate Aggregate SQL", CheckResult.Fail, e));
        }
    }

    private int? _orderWithinKnownContainer;
    private int? _overrideFiltersByUsingParentAggregateConfigurationInsteadID;
    private int? _rootFilterContainerID;

    private bool orderFetchAttempted;


    /// <summary>
    /// If the AggregateConfiguration is set up as a cohort identification set in a <see cref="CohortIdentificationConfiguration"/> then this method will return the set container
    /// (e.g. UNION / INTERSECT / EXCEPT) that it is in.  Returns null if it is not in a <see cref="CohortAggregateContainer"/>.
    /// </summary>
    /// <returns></returns>
    public CohortAggregateContainer GetCohortAggregateContainerIfAny() =>
        CatalogueRepository.CohortContainerManager.GetParent(this);

    /// <summary>
    /// All AggregateConfigurations have the potential a'Joinable Patient Index Table' (see AggregateConfiguration class documentation).  This method returns
    /// true if there is an associated JoinableCohortAggregateConfiguration that would make an ordinary AggregateConfiguration into a 'Patient Index Table'.
    /// </summary>
    /// <returns>true if the AggregateConfiguration is part of a cic fulfilling the role of 'Patient Index Table' as defined by the existence of a
    ///  JoinableCohortAggregateConfiguration object</returns>
    public bool IsJoinablePatientIndexTable() => JoinableCohortAggregateConfiguration != null;

    /// <summary>
    /// If the AggregateConfiguration is set up as a cohort identification set or patient index table then this method will return the associated
    /// <see cref="CohortIdentificationConfiguration"/> that it is a part of.
    /// </summary>
    /// <returns></returns>
    public CohortIdentificationConfiguration GetCohortIdentificationConfigurationIfAny()
    {
        //see if there is a container for this Aggregate
        var container = GetCohortAggregateContainerIfAny();


        if (container != null)
            return container.GetCohortIdentificationConfiguration();

        //it is not part of a container, maybe it is a joinable?
        var joinable = Repository.GetAllObjectsWithParent<JoinableCohortAggregateConfiguration>(this).SingleOrDefault();

        //it is a joinable (Patient Index Table) so return it
        return joinable?.CohortIdentificationConfiguration;
    }

    /// <summary>
    /// Creates a complete clone copy of the current AggregateConfiguration.  The clone will include all new AggregateDimensions, IFilters, IContainers etc.
    /// IMPORTANT: This method is designed for cohort identifying AggregateConfigurations only and therefore does not support Axis / TopX / Pivot.
    /// </summary>
    /// <returns></returns>
    public AggregateConfiguration CreateClone()
    {
        var cataRepo = CatalogueRepository;
        var clone = ShallowClone();

        if (clone.PivotOnDimensionID != null)
            throw new NotImplementedException("Cannot clone due to PIVOT");

        foreach (var aggregateDimension in AggregateDimensions)
        {
            var cloneDimension = new AggregateDimension((ICatalogueRepository)Repository,
                aggregateDimension.ExtractionInformation, clone)
            {
                Alias = aggregateDimension.Alias,
                SelectSQL = aggregateDimension.SelectSQL,
                Order = aggregateDimension.Order
            };
            cloneDimension.SaveToDatabase();

            if (aggregateDimension.AggregateContinuousDateAxis != null)
                throw new NotImplementedException("Cannot clone due to AXIS");
        }

        //now clone its AggregateForcedJoins
        foreach (var t in cataRepo.AggregateForcedJoinManager.GetAllForcedJoinsFor(this))
            cataRepo.AggregateForcedJoinManager.CreateLinkBetween(clone, t);

        if (RootFilterContainer_ID != null)
        {
            var clonedContainerSet = RootFilterContainer.DeepCloneEntireTreeRecursivelyIncludingFilters();
            clone.RootFilterContainer_ID = clonedContainerSet.ID;
        }

        foreach (var p in GetAllParameters())
        {
            var cloneP = new AnyTableSqlParameter(cataRepo, clone, p.ParameterSQL)
            {
                Comment = p.Comment,
                Value = p.Value
            };
            cloneP.SaveToDatabase();
        }


        clone.SaveToDatabase();

        return clone;
    }

    /// <summary>
    /// Using the set; on Order property changes the Order. if you want to informt his object of its Order because you already know what it is but this object doesn't know yet
    /// then you can use this method to force a specific known order onto the object in memory.
    /// </summary>
    /// <param name="currentOrder"></param>
    public void SetKnownOrder(int currentOrder)
    {
        orderFetchAttempted = true;
        _orderWithinKnownContainer = currentOrder;
    }

    /// <summary>
    /// Deletes the AggregateConfiguration.  This includes removing it from its <see cref="CohortAggregateContainer"/> if it is part of one.  Also includes deleting its
    /// <see cref="JoinableCohortAggregateConfiguration"/> if it is a 'patient index table'.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown if the AggregateConfiguration is a patient index table that is being used by other AggregateConfigurations</exception>
    public override void DeleteInDatabase()
    {
        var container = GetCohortAggregateContainerIfAny();

        container?.RemoveChild(this);

        var isAJoinable = Repository.GetAllObjectsWithParent<JoinableCohortAggregateConfiguration>(this)
            .SingleOrDefault();
        if (isAJoinable != null)
            if (isAJoinable.Users.Any())
                throw new NotSupportedException(
                    $"Cannot Delete AggregateConfiguration '{this}' because it is a Joinable Patient Index Table AND it has join users.  You must first remove the join usages and then you can delete it.");
            else
                isAJoinable.DeleteInDatabase();

        base.DeleteInDatabase();
    }


    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        var dependencies = new List<IHasDependencies>();
        dependencies.AddRange(AggregateDimensions);
        dependencies.AddRange(Parameters);
        dependencies.AddRange(ForcedJoins);
        dependencies.Add(Catalogue);

        return dependencies.ToArray();
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsDependingOnThis()
    {
        var dependers = new List<IHasDependencies>();

        var cic = GetCohortIdentificationConfigurationIfAny();
        if (cic != null)
            dependers.Add(cic);

        return dependers.ToArray();
    }

    /// <summary>
    /// Returns a description of the role of the <see cref="AggregateConfiguration"/> based on whether it is set up as a cohort, patient index table or cohort identification aggregate.
    /// </summary>
    /// <returns></returns>
    public string WhatIsThis()
    {
        if (IsJoinablePatientIndexTable())
            return
                @"This is an AggregateConfiguration running as a 'Joinable PatientIndex Table'.  its role is to produce a patient identifier fact table for joining to other Cohort Aggregates during cohort building (See JoinableCohortAggregateConfiguration)";

        return IsCohortIdentificationAggregate
            ? @"This is an AggregateConfiguration running as a 'Cohort Aggregate'.  its role is to produce a list of unique patient identifiers from a single dataset (e.g. 'all patients with HBA1c test code > 50 in biochemistry')"
            : @"This is an AggregateConfiguration running as an 'Aggregate Graph'.  its role is to produce summary information about a dataset designed to be displayed in a graph e.g. number of records each year by healthboard";
    }

    public AggregateConfiguration ShallowClone()
    {
        var clone = new AggregateConfiguration(CatalogueRepository, Catalogue, Name);
        CopyShallowValuesTo(clone);
        return clone;
    }

    public void CreateRootContainerIfNotExists()
    {
        if (RootFilterContainer_ID == null)
        {
            var container = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
            RootFilterContainer_ID = container.ID;
            SaveToDatabase();
        }
    }

    public ICatalogue GetCatalogue() => Catalogue;

    public IFilterFactory GetFilterFactory() => new AggregateFilterFactory(CatalogueRepository);
}