// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.IO;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;

namespace Rdmp.Core.DataLoad.Engine.Pipeline;

/// <summary>
///     Describes the use case of uploading a <see cref="FileInfo" /> to a target database server.  Compatible pipelines
///     for achieving this must have a destination
///     of (or inheriting from) <see cref="DataTableUploadDestination" /> and a source that implements IPipelineRequirement
///     &lt;FlatFileToLoad&gt;.
/// </summary>
public sealed class UploadFileUseCase : PipelineUseCase
{
    public UploadFileUseCase(FileInfo file, DiscoveredDatabase targetDatabase, IBasicActivateItems activator)
    {
        AddInitializationObject(new FlatFileToLoad(file));
        AddInitializationObject(targetDatabase);
        AddInitializationObject(activator);

        GenerateContext();
    }

    protected override IDataFlowPipelineContext GenerateContextImpl()
    {
        var context = new DataFlowPipelineContextFactory<DataTable>().Create(PipelineUsage.LoadsSingleFlatFile);
        context.MustHaveDestination = typeof(DataTableUploadDestination);
        return context;
    }

    private UploadFileUseCase() : base(new[]
    {
        typeof(FlatFileToLoad),
        typeof(DiscoveredDatabase),
        typeof(IBasicActivateItems)
    })
    {
        GenerateContext();
    }

    public static PipelineUseCase DesignTime()
    {
        return new UploadFileUseCase();
    }
}