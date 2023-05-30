// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;

namespace Rdmp.UI.PipelineUIs.Pipelines.PluginPipelineUsers;

/// <summary>
/// Turns an IDemandToUseAPipeline plugin class into an IPipelineUser and IPipelineUseCase (both) for use with PipelineSelectionUIFactory
/// </summary>
public sealed class PluginPipelineUser : PipelineUseCase,IPipelineUser
{
    private IPipelineUseCase _useCase;
    public PipelineGetter Getter { get; private set; }
    public PipelineSetter Setter { get; private set; }

    public PluginPipelineUser(RequiredPropertyInfo demand, ArgumentValueUIArgs args, object demanderInstance)
        : base(new Type[] { }) //makes it a design time use case
    {
        Getter = () =>
        {
            var p = (Pipeline)args.InitialValue;
            return p;
        };

        Setter = v =>args.Setter(v);

        var pipeDemander = demanderInstance as IDemandToUseAPipeline;

        if (pipeDemander == null)
            throw new NotSupportedException(
                $"Class {demanderInstance.GetType().Name} does not implement interface IDemandToUseAPipeline despite having a property which is a Pipeline");

        _useCase = pipeDemander.GetDesignTimePipelineUseCase(demand);
            
        ExplicitSource = _useCase.ExplicitSource;
        ExplicitDestination = _useCase.ExplicitDestination;
            
        foreach (var o in _useCase.GetInitializationObjects())
            AddInitializationObject(o);

        GenerateContext();
    }
        
    protected override IDataFlowPipelineContext GenerateContextImpl()
    {
        return _useCase.GetContext();
    }
}