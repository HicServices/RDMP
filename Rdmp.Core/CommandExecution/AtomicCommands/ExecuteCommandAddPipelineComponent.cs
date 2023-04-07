﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Icons.IconProvision;
using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using System.Linq;
using System.Reflection;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Adds new <see cref="PipelineComponent"/> to an existing <see cref="Pipeline"/>
/// </summary>
public class ExecuteCommandAddPipelineComponent : BasicCommandExecution
{
    private readonly IPipeline _pipeline;
    private readonly Type _toAdd;
    private readonly int? _order;
    private readonly IPipelineUseCase _useCaseIfAny;

    [UseWithObjectConstructor]
    public ExecuteCommandAddPipelineComponent(IBasicActivateItems activator, 
        [DemandsInitialization("The pipeline to add the component to")]
        IPipeline pipeline,
        [DemandsInitialization("The Type of component to add")]
        Type toAdd,
        [DemandsInitialization("The position in the pipeline to place the component relative to other components.")]
        int? order = null) : base(activator)
    {
        _pipeline = pipeline;
        _toAdd = toAdd;
        _order = order;
    }

    public ExecuteCommandAddPipelineComponent(IBasicActivateItems activator, IPipeline pipeline, IPipelineUseCase useCaseIfAny) : base(activator)
    {
        _pipeline = pipeline;
        _useCaseIfAny = useCaseIfAny;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.PipelineComponent,OverlayKind.Add);
    }

    public override void Execute()
    {
        base.Execute();
        var add = _toAdd;
        var order = _order;

        // if command doesn't know which to add, ask user
        if (add == null)
        {
            var mef = BasicActivator.RepositoryLocator.CatalogueRepository.MEF;
            var context = _useCaseIfAny?.GetContext();
            List<Type> offer = new List<Type>();

            TypeFilter filter = (t, o) => t.IsGenericType &&
                                          (t.GetGenericTypeDefinition() == typeof(IDataFlowComponent<>) ||
                                           t.GetGenericTypeDefinition() == typeof(IDataFlowSource<>));

            //get any source and flow components compatible with any context
            offer.AddRange(
                mef.GetAllTypes()
                    .Where(t => !t.IsInterface && !t.IsAbstract)
                    .Where(t => t.FindInterfaces(filter, null).Any())
                    .Where(t=> context == null || context.IsAllowable(t))
            );

            if (!BasicActivator.SelectObject("Add", offer.ToArray(), out add))
                return;
        }

        // Only proceed if we have a component type to add to the pipe
        if (add == null) return;

        // check if it is a source or destination (or if both are false it is a middle component)
        TypeFilter sourceFilter = (t, o) =>
            t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDataFlowSource<>);
        TypeFilter destFilter = (t, o) =>
            t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDataFlowDestination<>);

        var isSource = add.FindInterfaces(sourceFilter, null).Any();
        var isDest = add.FindInterfaces(destFilter, null).Any();

        if (isSource)
        {
            order = int.MinValue;
            if (_pipeline.SourcePipelineComponent_ID.HasValue)
            {
                throw new Exception($"Pipeline '{_pipeline}' already has a source");
            }
        }

        if (isDest)
        {
            order = int.MaxValue;
            if (_pipeline.DestinationPipelineComponent_ID.HasValue)
            {
                throw new Exception($"Pipeline '{_pipeline}' already has a destination");
            }
        }

        // if we don't know the order yet and it's important
        if (!order.HasValue && !isDest && !isSource)
        {
            if (BasicActivator.SelectValueType("Order", typeof(int), 0, out object chosen))
            {
                order = (int)chosen;
            }
            else
            {
                return;
            }
        }

        var newComponent = new PipelineComponent(BasicActivator.RepositoryLocator.CatalogueRepository, _pipeline,
            add, order ?? 0);
        newComponent.CreateArgumentsForClassIfNotExists(add);

        if (isSource)
        {
            _pipeline.SourcePipelineComponent_ID = newComponent.ID;
            _pipeline.SaveToDatabase();
        }

        if (isDest)
        {
            _pipeline.DestinationPipelineComponent_ID = newComponent.ID;
            _pipeline.SaveToDatabase();
        }

        Publish(newComponent);
        Emphasise(newComponent);
    }
}