// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///     Allows you to generate adjusted AggregateBuilders in which a basic AggregateBuilder from an AggregateConfiguration
///     is adjusted to include an inception WHERE statement
///     which restricts the results to only those patients who are in a cohort (the cohort is the list of private
///     identifiers returned by the AggregateConfiguration passed
///     into the constructor as the 'cohort' argument)
/// </summary>
public class CohortSummaryQueryBuilder
{
    private readonly AggregateConfiguration _summary;

    private readonly ISqlParameter[] _globals;
    private readonly IColumn _extractionIdentifierColumn;

    private readonly AggregateConfiguration _cohort;
    private readonly ICoreChildProvider _childProvider;
    private readonly CohortAggregateContainer _cohortContainer;

    /// <summary>
    ///     Read class description to see what the class does, use this constructor to specify an Aggregate graph and a cohort
    ///     with which to restrict it.  The cohort
    ///     aggregate must return a list of private identifiers.  The parameters must belong to the same Catalogue (dataset).
    /// </summary>
    /// <param name="summary">
    ///     A basic aggregate that you want to restrict by cohort e.g. a pivot on drugs prescribed over time
    ///     with an axis interval of year
    /// </param>
    /// <param name="cohort">
    ///     A cohort aggregate that has a single AggregateDimension which must be an IsExtractionIdentifier
    ///     and must follow the correct cohort aggregate naming conventions (See IsCohortIdentificationAggregate)
    /// </param>
    /// <param name="childProvider"></param>
    public CohortSummaryQueryBuilder(AggregateConfiguration summary, AggregateConfiguration cohort,
        ICoreChildProvider childProvider)
    {
        if (cohort == null)
            throw new ArgumentException("cohort was null in CohortSummaryQueryBuilder constructor", nameof(cohort));

        if (summary.Equals(cohort))
            throw new ArgumentException(
                "Summary and Cohort should be different aggregates.  Summary should be a graphable useful aggregate while cohort should return a list of private identifiers");

        ThrowIfNotValidGraph(summary);

        try
        {
            ThrowIfNotCohort(cohort);
        }
        catch (Exception e)
        {
            throw new ArgumentException(
                $"The second argument to constructor CohortSummaryQueryBuilder should be a cohort identification aggregate (i.e. have a single AggregateDimension marked IsExtractionIdentifier and have a name starting with {CohortIdentificationConfiguration.CICPrefix}) but the argument you passed ('{cohort}') was NOT a cohort identification configuration aggregate",
                e);
        }

        if (summary.Catalogue_ID != cohort.Catalogue_ID)
            throw new ArgumentException(
                $"Constructor arguments to CohortSummaryQueryBuilder must belong to the same dataset (i.e. have the same underlying Catalogue), the first argument (the graphable aggregate) was called '{summary} and belonged to Catalogue ID {summary.Catalogue_ID} while the second argument (the cohort) was called '{cohort}' and belonged to Catalogue ID {cohort.Catalogue_ID}");

        _summary = summary;
        _cohort = cohort;
        _childProvider = childProvider;

        //here we take the identifier from the cohort because the dataset might have multiple identifiers e.g. birth record could have patient Id, parent Id, child Id etc.  The Aggregate will already have one of those selected and only one of them selected
        _extractionIdentifierColumn = _cohort.AggregateDimensions.Single(d => d.IsExtractionIdentifier);

        var cic = _cohort.GetCohortIdentificationConfigurationIfAny() ?? throw new ArgumentException(
            $"AggregateConfiguration {_cohort} looked like a cohort but did not belong to any CohortIdentificationConfiguration");
        _globals = cic.GetAllParameters();
    }


    public CohortSummaryQueryBuilder(AggregateConfiguration summary, CohortAggregateContainer cohortAggregateContainer)
    {
        ThrowIfNotValidGraph(summary);

        var extractionIdentifiers = summary.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .Where(e => e.IsExtractionIdentifier).ToArray();

        if (extractionIdentifiers.Length != 1)
            throw new Exception(
                $"Aggregate Graph '{summary} cannot be used to graph the extraction identifiers of cohort aggregate container '{cohortAggregateContainer}' because it has {extractionIdentifiers.Length} IsExtractionIdentifier columns");

        _extractionIdentifierColumn = extractionIdentifiers.Single();
        _summary = summary;
        _cohortContainer = cohortAggregateContainer;

        var cic = _cohortContainer.GetCohortIdentificationConfiguration() ?? throw new ArgumentException(
            $"CohortAggregateContainer {cohortAggregateContainer} is an orphan? it does not belong to any CohortIdentificationConfiguration");
        _globals = cic.GetAllParameters();
    }

    /// <summary>
    ///     Functions in two modes
    ///     <para>
    ///         WhereExtractionIdentifiersIn:
    ///         Returns a adjusted AggregateBuilder that is based on the summary AggregateConfiguration but which has an
    ///         inception WHERE statement that restricts the IsExtractionIdentifier column
    ///         by those values returned by the Cohort query.  In order that this query doesn't become super insane we require
    ///         that the Cohort be cached so that it is just a simple single
    ///         like IFilter e.g. conceptually: WHERE CHI IN (Select CHI from
    ///         IndexedExtractionIdentifierList_AggregateConfiguration5)
    ///     </para>
    ///     <para>
    ///         WhereRecordsIn
    ///         Returns an adjusted AggregateBuilder that is based on the summary AggregateConfiguration but which has an root
    ///         AND container which includes both the container tree of the summary
    ///         and the cohort (resulting in a graphing of the RECORDS returned by the cohort set query instead of a master set
    ///         of all those patients records - as above does).
    ///     </para>
    /// </summary>
    /// <returns></returns>
    public AggregateBuilder GetAdjustedAggregateBuilder(CohortSummaryAdjustment adjustment,
        IFilter singleFilterOnly = null)
    {
        switch (adjustment)
        {
            case CohortSummaryAdjustment.WhereExtractionIdentifiersIn:
                if (singleFilterOnly != null)
                    throw new NotSupportedException(
                        "You cannot graph a single IFilter with CohortSummaryAdjustment.WhereExtractionIdentifiersIn");

                return GetAdjustedForExtractionIdentifiersIn();
            case CohortSummaryAdjustment.WhereRecordsIn:
                return GetAdjustedForRecordsIn(singleFilterOnly);
            default:
                throw new ArgumentOutOfRangeException(nameof(adjustment));
        }
    }

    private AggregateBuilder GetAdjustedForRecordsIn(IFilter singleFilterOnly = null)
    {
        if (_cohort == null)
            throw new NotSupportedException(
                "This method only works when there is a cohort aggregate, it does not work for CohortAggregateContainers");

        var memoryRepository = new MemoryCatalogueRepository();

        //Get a builder for creating the basic aggregate graph
        var summaryBuilder = _summary.GetQueryBuilder();

        //Find its root container if it has one
        var summaryRootContainer = summaryBuilder.RootFilterContainer;

        //work out a filter SQL that will restrict the graph generated only to the cohort
        var cohortRootContainer = _cohort.RootFilterContainer;

        //if we are only graphing a single filter from the Cohort
        if (singleFilterOnly != null)
            cohortRootContainer = new SpontaneouslyInventedFilterContainer(memoryRepository, null,
                new[] { singleFilterOnly }, FilterContainerOperation.AND);

        var joinUse = _cohort.PatientIndexJoinablesUsed.SingleOrDefault();
        var joinTo = joinUse?.JoinableCohortAggregateConfiguration?.AggregateConfiguration;

        //if there is a patient index table we must join to it
        if (joinUse != null)
        {
            //get sql for the join table
            var builder = new CohortQueryBuilder(joinTo, _globals, null);
            var joinableSql = new CohortQueryBuilderDependencySql(builder.SQL, builder.ParameterManager);

            var extractionIdentifierColumn = _summary.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
                .Where(ei => ei.IsExtractionIdentifier).ToArray();

            if (extractionIdentifierColumn.Length != 1)
                throw new Exception(
                    $"Catalogue behind {_summary} must have exactly 1 IsExtractionIdentifier column but it had {extractionIdentifierColumn.Length}");

            CohortQueryBuilderHelper.AddJoinToBuilder(_summary, extractionIdentifierColumn[0], summaryBuilder,
                new QueryBuilderArgs(joinUse, joinTo, joinableSql, null, _globals));
        }

        //if the cohort has no WHERE SQL
        if (cohortRootContainer == null)
            return summaryBuilder; //summary can be run verbatim

        //the summary has no WHERE SQL
        if (summaryRootContainer == null)
        {
            summaryBuilder.RootFilterContainer = cohortRootContainer; //hijack the cohorts root container
        }
        else
        {
            //they both have WHERE SQL

            //Create a new spontaneous container (virtual memory only container) that contains both subtrees
            var spontContainer = new SpontaneouslyInventedFilterContainer(memoryRepository,
                new[] { cohortRootContainer, summaryRootContainer }, null, FilterContainerOperation.AND);
            summaryBuilder.RootFilterContainer = spontContainer;
        }

        //better import the globals because WHERE logic from the cohort has been inherited... only problem will be if there are conflicting globals in users aggregate but that's just tough luck
        foreach (var p in _globals)
            summaryBuilder.ParameterManager.AddGlobalParameter(p);

        return summaryBuilder;
    }

    private AggregateBuilder GetAdjustedForExtractionIdentifiersIn()
    {
        var cachingServer = GetQueryCachingServer() ??
                            throw new NotSupportedException("No Query Caching Server configured");
        var memoryRepository = new MemoryCatalogueRepository();

        //Get a builder for creating the basic aggregate graph
        var builder = _summary.GetQueryBuilder();

        //Find its root container if it has one
        var oldRootContainer = builder.RootFilterContainer;

        //Create a new spontaneous container (virtual memory only container, this will include an in line filter that restricts the graph to match the cohort and then include a subcontainer with the old root container - if there was one)
        var spontContainer = new SpontaneouslyInventedFilterContainer(memoryRepository,
            oldRootContainer != null ? new[] { oldRootContainer } : null, null, FilterContainerOperation.AND);

        //work out a filter SQL that will restrict the graph generated only to the cohort
        var cohortQueryBuilder = GetBuilder();
        cohortQueryBuilder.CacheServer = cachingServer;

        //It is coming direct from the cache so we don't need to output any parameters... the only ones that would appear are the globals anyway and those are not needed since cache
        cohortQueryBuilder.DoNotWriteOutParameters = true;
        //the basic cohort SQL select chi from dataset where ....
        var cohortSql = cohortQueryBuilder.SQL;

        if (cohortQueryBuilder.Results.CountOfCachedSubQueries == 0 || cohortQueryBuilder.Results.CountOfSubQueries !=
            cohortQueryBuilder.Results.CountOfCachedSubQueries)
            throw new NotSupportedException(
                $"Only works for 100% Cached queries, your query has {cohortQueryBuilder.Results.CountOfCachedSubQueries}/{cohortQueryBuilder.Results.CountOfSubQueries} queries cached");

        //there will be a single dimension on the cohort aggregate so this translates to "MyTable.MyDataset.CHI in Select(
        var filterSql = $"{_extractionIdentifierColumn.SelectSQL} IN ({cohortSql})";

        //Add a filter which restricts the graph generated to the cohort only
        spontContainer.AddChild(new SpontaneouslyInventedFilter(memoryRepository, spontContainer, filterSql,
            "Patient is in cohort",
            "Ensures the patients in the summary aggregate are also in the cohort aggregate (and only them)", null));

        builder.RootFilterContainer = spontContainer;

        return builder;
    }

    private ExternalDatabaseServer GetQueryCachingServer()
    {
        if (_cohort != null)
            return _cohort.GetCohortIdentificationConfigurationIfAny().QueryCachingServer;

        return _cohortContainer != null
            ? _cohortContainer.GetCohortIdentificationConfiguration().QueryCachingServer
            : throw new NotSupportedException("Expected there to be either a _cohort or a _cohortContainer");
    }

    private CohortQueryBuilder GetBuilder()
    {
        if (_cohort != null)
            return new CohortQueryBuilder(_cohort, _globals, _childProvider);

        return _cohortContainer != null
            ? new CohortQueryBuilder(_cohortContainer, _globals, _childProvider)
            : throw new NotSupportedException("Expected there to be either a _cohort or a _cohortContainer");
    }

    public static AggregateConfiguration[] GetAllCompatibleSummariesForCohort(AggregateConfiguration cohort)
    {
        ThrowIfNotCohort(cohort);

        return cohort.Catalogue.AggregateConfigurations.Where(a => !a.IsCohortIdentificationAggregate).ToArray();
    }


    private static void ThrowIfNotCohort(AggregateConfiguration cohort)
    {
        if (!cohort.IsCohortIdentificationAggregate)
            throw new ArgumentException(
                $"AggregateConfiguration {cohort} was a not a cohort identification configuration aggregate its name didn't start with '{CohortIdentificationConfiguration.CICPrefix}', this is not allowed, the second argument must always be a cohort specific aggregate with only a single column marked IsExtractionIdentifier etc");

        if (cohort.AggregateDimensions.Count(d => d.IsExtractionIdentifier) != 1)
            throw new Exception(
                $"Expected cohort {cohort} to have exactly 1 column which would be an IsExtractionIdentifier");
    }

    private static void ThrowIfNotValidGraph(AggregateConfiguration summary)
    {
        if (summary == null)
            throw new ArgumentException("summary was null in CohortSummaryQueryBuilder constructor", nameof(summary));

        if (summary.IsCohortIdentificationAggregate)
            throw new ArgumentException(
                $"The first argument to constructor CohortSummaryQueryBuilder should be a basic AggregateConfiguration (i.e. not a cohort) but the argument you passed ('{summary}') was a cohort identification configuration aggregate");
    }
}

public enum CohortSummaryAdjustment
{
    WhereExtractionIdentifiersIn,
    WhereRecordsIn
}