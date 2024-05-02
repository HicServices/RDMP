// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.Curation.Data.Pipelines;

/// <summary>
///     Describes an IDataFlowComponent which may or may not be compatible with a specific DataFlowPipelineContext.  It
///     describes how/if its requirements conflict with the context
///     e.g. a DelimitedFlatFileDataFlowSource requires a FlatFileToLoad and is therefore incompatible under any context
///     where that object is not available.
/// </summary>
public class AdvertisedPipelineComponentTypeUnderContext
{
    private readonly bool _allowableUnderContext;
    private readonly string _allowableReason;

    private readonly PipelineComponentRole _role;
    private readonly Type _componentType;


    private readonly List<Type> unmetRequirements = new();

    public AdvertisedPipelineComponentTypeUnderContext(Type componentType, IPipelineUseCase useCase)
    {
        _componentType = componentType;

        _role = PipelineComponent.GetRoleFor(componentType);

        var context = useCase.GetContext();

        _allowableUnderContext = context.IsAllowable(componentType, out _allowableReason);

        Type[] initializationTypes;

        var initializationObjects = useCase.GetInitializationObjects();

        //it is permitted to specify only Types as initialization objects if it is design time and the user hasn't picked any objects to execute the use case under
        if (useCase.IsDesignTime && initializationObjects.All(t => t is Type))
            initializationTypes = initializationObjects.Cast<Type>().ToArray();
        else
            initializationTypes = useCase.GetInitializationObjects().Select(o => o.GetType()).ToArray();

        foreach (var requiredInputType in context.GetIPipelineRequirementsForType(componentType))
            //if there are no initialization objects that are instances of an IPipelineRequirement<T> then we cannot satisfy the components pipeline requirements (e.g. a component  DelimitedFlatFileDataFlowSource requires a FlatFileToLoad but pipeline is trying to load from a database reference)
            if (!initializationTypes.Any(available =>
                    requiredInputType == available || requiredInputType.IsAssignableFrom(available)))
                unmetRequirements.Add(requiredInputType);
    }

    public Type GetComponentType()
    {
        return _componentType;
    }

    public string Namespace()
    {
        return _componentType.Namespace;
    }

    public override string ToString()
    {
        return _componentType.Name;
    }

    public PipelineComponentRole GetRole()
    {
        return _role;
    }

    public string UIDescribeCompatible()
    {
        return _allowableUnderContext && !unmetRequirements.Any() ? "Yes" : "No";
    }

    public bool IsCompatible()
    {
        return _allowableUnderContext && !unmetRequirements.Any();
    }

    public string GetReasonIncompatible()
    {
        var toReturn = _allowableReason;

        if (unmetRequirements.Any())
        {
            if (!string.IsNullOrWhiteSpace(toReturn))
                toReturn += " and the";
            else
                toReturn += "The";

            toReturn +=
                $" following types are required by the component but not available as input objects to the pipeline {string.Join(",", unmetRequirements)}";
        }

        return toReturn;
    }
}