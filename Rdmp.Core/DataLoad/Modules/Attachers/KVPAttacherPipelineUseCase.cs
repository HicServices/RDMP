// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;

namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
///     Use case for the user configured pipeline for reading from a flat file.  Used by KVPAttacher (See KVPAttacher) to
///     allow the user control over how the
///     source file format is read (e.g. csv, fixed width, excel etc).
/// </summary>
public sealed class KVPAttacherPipelineUseCase : PipelineUseCase
{
    public KVPAttacherPipelineUseCase(KVPAttacher kvpAttacher, FlatFileToLoad file)
    {
        ExplicitDestination = kvpAttacher;
        AddInitializationObject(file);

        GenerateContext();
    }

    protected override IDataFlowPipelineContext GenerateContextImpl()
    {
        var context = new DataFlowPipelineContextFactory<DataTable>().Create(PipelineUsage.FixedDestination);
        context.MustHaveSource = typeof(IDataFlowSource<DataTable>);

        return context;
    }
}