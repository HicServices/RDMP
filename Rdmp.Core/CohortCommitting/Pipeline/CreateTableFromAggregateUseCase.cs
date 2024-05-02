// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using FAnsi.Discovery;
using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CohortCommitting.Pipeline;

/// <summary>
///     Use case which describes creating a new table in the database containing all rows matched by the
///     <see cref="AggregateConfiguration" />.
///     The source is fixed the destination and middle components are open.
/// </summary>
public sealed class CreateTableFromAggregateUseCase : PipelineUseCase
{
    /// <summary>
    ///     Defines a new use case in which the given <see cref="AggregateConfiguration" /> will be turned into an SQL query
    ///     and used to generate rows
    ///     that will be released into the pipeline.  The source is fixed the destination and middle components are open.
    /// </summary>
    /// <param name="aggregateConfiguration">The aggregate query that will be run to generate the rows</param>
    /// <param name="constrainByCohort">
    ///     Only applies if <see cref="AggregateConfiguration" /> is a patient index table, specifying a cohort will only
    ///     commit rows
    ///     in which the patient id appears in the cohort
    /// </param>
    /// <param name="table">
    ///     The destination table in which to put the matched records.
    ///     <para>
    ///         (table does not have to exist yet, you can use <see cref="DiscoveredDatabase.ExpectTable" /> to obtain a
    ///         reference to a non existant table)
    ///     </para>
    /// </param>
    public CreateTableFromAggregateUseCase(AggregateConfiguration aggregateConfiguration,
        ExtractableCohort constrainByCohort, DiscoveredTable table)
    {
        if (constrainByCohort == null)
        {
            var src = new AggregateConfigurationTableSource();
            src.PreInitialize(aggregateConfiguration, ThrowImmediatelyDataLoadEventListener.Quiet);
            src.TableName = table.GetRuntimeName();
            ExplicitSource = src;
        }
        else
        {
            AddInitializationObject(constrainByCohort);

            var src = new PatientIndexTableSource();
            src.PreInitialize(aggregateConfiguration, ThrowImmediatelyDataLoadEventListener.Quiet);
            src.PreInitialize(constrainByCohort, ThrowImmediatelyDataLoadEventListener.Quiet);
            src.TableName = table.GetRuntimeName();
            ExplicitSource = src;
        }

        AddInitializationObject(aggregateConfiguration);
        AddInitializationObject(aggregateConfiguration.Repository);
        AddInitializationObject(table.Database);

        GenerateContext();
    }

    protected override IDataFlowPipelineContext GenerateContextImpl()
    {
        var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
        var context = contextFactory.Create(PipelineUsage.FixedSource);
        context.MustHaveDestination = typeof(DataTableUploadDestination);

        return context;
    }

    /// <summary>
    ///     Design time types
    /// </summary>
    private CreateTableFromAggregateUseCase()
        : base(new[]
        {
            typeof(AggregateConfiguration),
            typeof(ExtractableCohort),
            typeof(DiscoveredDatabase),
            typeof(ICatalogueRepository)
        })
    {
        ExplicitSource = new AggregateConfigurationTableSource();
        GenerateContext();
    }

    public static PipelineUseCase DesignTime(ICatalogueRepository catalogueRepository)
    {
        return new CreateTableFromAggregateUseCase();
    }
}