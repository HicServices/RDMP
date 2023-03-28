// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.Pipelines;

namespace Rdmp.UI.PipelineUIs.Pipelines;

/// <summary>
/// Allows you to pick an <see cref="IPipeline"/> (or create a new one) to achieve a data flow task (e.g. load a file as a new dataset or attach
/// custom data to a cohort etc - See <see cref="IPipelineUseCase"/>).
/// 
/// <para>If you cannot see the pipeline you expected to see then it is possible that the pipeline is broken or somehow otherwise incompatible with the
/// current context.  If this is the case then you can untick 'Only Show Compatible Pipelines' which will show all Pipelines of the type T (usually DataTable).
/// You should only use this feature to edit Pipelines as there is zero chance they will execute Successfully if they are not compatible with the
/// DataFlowPipelineContext.</para>
/// </summary>
public interface IPipelineSelectionUI
{
    event EventHandler PipelineChanged;
    IPipeline Pipeline { get; set; }
        
    void CollapseToSingleLineMode();
}