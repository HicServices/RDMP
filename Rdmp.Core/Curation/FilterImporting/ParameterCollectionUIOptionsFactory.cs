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
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.QueryBuilding.Parameters;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.FilterImporting;

/// <summary>
///     Creates instances of <see cref="ParameterCollectionUIOptions" /> based on the Type of
///     <see cref="ICollectSqlParameters" /> host.
/// </summary>
public class ParameterCollectionUIOptionsFactory
{
    private const string UseCaseIFilter =
        "You are editing a filter which will be used in a WHERE SQL statement.  This control will let you view which higher level parameters (if any) you can use in your query.  It also lets you change the values/datatypes of any parameters you have used in your Filter. To create a new Parameter just type its name as you normally would into the Filter dialog (above).";

    private const string UseCaseTableInfo =
        "You are viewing the SQL parameters associated with a Table Valued Function.  You should ONLY change comment/value on these parameters. If you have changed the underlying database implementation of your TableValuedFunction and want to adjust parameters here accordingly then you should instead use the 'Synchronize TableInfo' command.";

    private const string UseCaseAggregateConfiguration =
        "You are trying to configure an Aggregate either as part of a cohort identification or as a graph/breakdown of a dataset.  This dialog shows you all the SQL parameters that are declared in any of the Filters you have created on this dataset as well as all the global overriding parameters that exist in the scope of your task.  If you have a lot of filters that for some reason share the same parameter, you can declare it at this level instead of explicitly under each filter.  Also this is the place you should declare parameter values if your dataset is a Table Valued Function, set values appropriate to your use case (these will override the defaults configured for that table).";

    private const string UseCaseParameterValueSet =
        "You are editing a 'pre canned' useful set of values for use with a Filter that has 1 or more parameters.  Each ExtractionFilter can have zero or more parameters which should have appropriate sample values but often you will want to record specific values e.g. for a filter 'Hospital Admissions with condition code X' you might want to record parameter value sets 'Dementia' as @codelist= 'A01,B01,C212' and 'Cancer' as @codelist='D21,E12,F2' etc.  This avoids creating duplicate filters while still allowing you to centralise such concept implementations into the Catalogue.  IMPORTANT: Only Value can be edited because Comment/Declaration are identical to the parent ExtractionFilterParameter.";

    private const string UseCaseCohortIdentificationConfiguration =
        "You are trying to build a cohort by performing SQL set operations on number of datasets, each dataset can have many filters which can have parameters.  It is likely that your datasets contain filters (e.g. 'only records from Tayside').  These filters may contain duplicate parameters (e.g. if you have 5 datasets each filtered by healthboard each with a parameter called @healthboard).  This dialog lets you configure a single 'overriding' master copy at the 'Cohort Identification Configuration' level which will allow you to change all copies at once in one place";

    private const string UseCaseExtractionConfigurationGlobals =
        @"You are trying to perform a data extraction of one or more datasets against a cohort.  It is likely that your datasets contain filters (e.g. 'only records from Tayside').  These filters may contain duplicate parameters (e.g. if you have 5 datasets each filtered by healthboard each with a parameter called @healthboard).  This dialog lets you configure a single 'overriding' master copy at the ExtractionConfiguration level which will allow you to change all copies at once in one place.  You will also see two global parameters the system generates automatically when doing extractions these are @CohortDefinitionID and @ProjectNumber";


    public static ParameterCollectionUIOptions Create(IFilter value, ISqlParameter[] globalFilterParameters)
    {
        var pm = new ParameterManager();

        foreach (var globalFilterParameter in globalFilterParameters)
            pm.AddGlobalParameter(globalFilterParameter);

        pm.AddParametersFor(value, ParameterLevel.QueryLevel);

        return new ParameterCollectionUIOptions(UseCaseIFilter, value, ParameterLevel.QueryLevel, pm);
    }

    public static ParameterCollectionUIOptions Create(ITableInfo tableInfo)
    {
        var pm = new ParameterManager();
        pm.AddParametersFor(tableInfo);
        return new ParameterCollectionUIOptions(UseCaseTableInfo, tableInfo, ParameterLevel.TableInfo, pm);
    }

    public static ParameterCollectionUIOptions Create(ExtractionFilterParameterSet parameterSet)
    {
        var pm = new ParameterManager();
        pm.ParametersFoundSoFarInQueryGeneration[ParameterLevel.TableInfo].AddRange(parameterSet.Values);

        return new ParameterCollectionUIOptions(UseCaseParameterValueSet, parameterSet, ParameterLevel.TableInfo, pm);
    }

    public static ParameterCollectionUIOptions Create(AggregateConfiguration aggregateConfiguration,
        ICoreChildProvider coreChildProvider)
    {
        ParameterManager pm;

        if (aggregateConfiguration.IsCohortIdentificationAggregate)
        {
            //Add the globals if it is part of a CohortIdentificationConfiguration
            var cic = aggregateConfiguration.GetCohortIdentificationConfigurationIfAny();

            var globals = cic != null ? cic.GetAllParameters() : Array.Empty<ISqlParameter>();

            var builder = new CohortQueryBuilder(aggregateConfiguration, globals, coreChildProvider);
            pm = builder.ParameterManager;

            try
            {
                //Generate the SQL which will make it find all the other parameters
                builder.RegenerateSQL();
            }
            catch (QueryBuildingException)
            {
                //if there's a problem with parameters or anything else, it is okay this dialog might let the user fix that
            }
        }
        else
        {
            var builder = aggregateConfiguration.GetQueryBuilder();
            pm = builder.ParameterManager;

            try
            {
                //Generate the SQL which will make it find all the other parameters
                builder.RegenerateSQL();
            }
            catch (QueryBuildingException)
            {
                //if there's a problem with parameters or anything else, it is okay this dialog might let the user fix that
            }
        }


        return new ParameterCollectionUIOptions(UseCaseAggregateConfiguration, aggregateConfiguration,
            ParameterLevel.CompositeQueryLevel, pm);
    }


    public ParameterCollectionUIOptions Create(ICollectSqlParameters host, ICoreChildProvider coreChildProvider)
    {
        return host switch
        {
            TableInfo tableInfo => Create(tableInfo),
            ExtractionFilterParameterSet extractionFilterParameterSet => Create(extractionFilterParameterSet),
            AggregateConfiguration aggregateConfiguration => Create(aggregateConfiguration, coreChildProvider),
            IFilter filter => Create(filter,
                FilterUIOptionsFactory.Create(filter).GetGlobalParametersInFilterScope()),
            CohortIdentificationConfiguration cohortIdentificationConfiguration => Create(
                cohortIdentificationConfiguration, coreChildProvider),
            ExtractionConfiguration extractionConfiguration => Create(extractionConfiguration),
            _ => throw new ArgumentException(
                "Host Type was not recognised as one of the Types we know how to deal with", nameof(host))
        };
    }

    private static ParameterCollectionUIOptions Create(
        CohortIdentificationConfiguration cohortIdentificationConfiguration, ICoreChildProvider coreChildProvider)
    {
        var builder = new CohortQueryBuilder(cohortIdentificationConfiguration, coreChildProvider);
        builder.RegenerateSQL();

        var paramManager = builder.ParameterManager;

        return new ParameterCollectionUIOptions(UseCaseCohortIdentificationConfiguration,
            cohortIdentificationConfiguration, ParameterLevel.Global, paramManager);
    }

    private ParameterCollectionUIOptions Create(ExtractionConfiguration extractionConfiguration)
    {
        var globals = extractionConfiguration.GlobalExtractionFilterParameters;
        var paramManager = new ParameterManager(globals);


        foreach (var selectedDatasets in extractionConfiguration.SelectedDataSets)
        {
            var rootFilterContainer = selectedDatasets.RootFilterContainer;

            if (rootFilterContainer != null)
            {
                var allFilters = SqlQueryBuilderHelper.GetAllFiltersUsedInContainerTreeRecursively(rootFilterContainer)
                    .ToList();
                paramManager.AddParametersFor(allFilters); //query level
            }
        }

        return new ParameterCollectionUIOptions(UseCaseExtractionConfigurationGlobals, extractionConfiguration,
            ParameterLevel.Global, paramManager, CreateNewParameterForExtractionConfiguration);
    }

    private ISqlParameter CreateNewParameterForExtractionConfiguration(ICollectSqlParameters collector,
        string parameterName)
    {
        if (!parameterName.StartsWith("@"))
            parameterName = $"@{parameterName}";

        var ec = (ExtractionConfiguration)collector;
        return new GlobalExtractionFilterParameter((IDataExportRepository)ec.Repository, ec,
            AnyTableSqlParameter.GetDefaultDeclaration(parameterName));
    }
}