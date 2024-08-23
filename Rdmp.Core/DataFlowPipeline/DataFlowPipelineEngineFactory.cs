// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataFlowPipeline.Requirements.Exceptions;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataFlowPipeline;

/// <summary>
/// Creates DataFlowPipelineEngines from IPipelines.  An IPipeline is the persistent user configured reusable list of components (and arguments for those components) which
/// will achieve a given task for the user (e.g. import a csv file).  The DataFlowPipelineContext defines both the Generic flow object of the engine (T) and which IPipelines
/// will be judged compatible (based on PreInitialize requirements etc).  Some contexts require a specific source/destination component that is available only at runtime
/// and cannot be changed/configured by the user (FixedDestination/FixedSource).  If the context requires a FixedSource or FixedDestination then you must pass the ExplicitSource
/// object / ExplicitDestination object into the constructor.
/// 
/// <para>In general rather than trying to use this class directly you should package up your requirements/initialization objects into a PipelineUseCase and call GetEngine. </para>
/// </summary>
public class DataFlowPipelineEngineFactory : IDataFlowPipelineEngineFactory
{
    private readonly IDataFlowPipelineContext _context;
    private IPipelineUseCase _useCase;

    private Type _engineType;

    /// <summary>
    /// Creates a new factory which can translate <see cref="IPipeline"/> blueprints into runnable <see cref="IDataFlowPipelineEngine"/> instances.
    /// </summary>
    /// <param name="useCase">The use case which describes which <see cref="IPipeline"/> are compatible, which objects are available for hydration/preinitialization etc</param>
    public DataFlowPipelineEngineFactory(IPipelineUseCase useCase)
    {
        _context = useCase.GetContext();
        _useCase = useCase;
        var flowType = _context.GetFlowType();
        _engineType = typeof(DataFlowPipelineEngine<>).MakeGenericType(flowType);
    }

    /// <inheritdoc/>
    public DataFlowPipelineEngineFactory(IPipelineUseCase useCase, IPipeline pipeline) : this(useCase)
    {
    }

    /// <inheritdoc/>
    public IDataFlowPipelineEngine Create(IPipeline pipeline, IDataLoadEventListener listener)
    {
        if (!_context.IsAllowable(pipeline, out var reason))
            throw new Exception($"Cannot create pipeline because: {reason}");

        var destination = GetBest(_useCase.ExplicitDestination, CreateDestinationIfExists(pipeline), "destination");
        var source = GetBest(_useCase.ExplicitSource, CreateSourceIfExists(pipeline), "source");

        //engine (this is the source, target is the destination)
        var dataFlowEngine =
            (IDataFlowPipelineEngine)ObjectConstructor.ConstructIfPossible(_engineType, _context, source, destination,
                listener, pipeline);

        //now go fetch everything that the user has configured for this particular pipeline except the source and destination
        //get the factory to realize the freaky Export types defined in any assembly anywhere and set their DemandsInitialization properties based on the Arguments
        foreach (var component in pipeline.PipelineComponents.Where(pc =>
                         pc.ID != pipeline.DestinationPipelineComponent_ID &&
                         pc.ID != pipeline.SourcePipelineComponent_ID)
                     .Select(CreateComponent))
            dataFlowEngine.ComponentObjects.Add(component);

        return dataFlowEngine;
    }

    /// <summary>
    /// Returns the thing that is not null or throws an exception because both are blank.  also throws if both are populated
    /// </summary>
    /// <typeparam name="T2"></typeparam>
    /// <param name="explicitThing"></param>
    /// <param name="pipelineConfigurationThing"></param>
    /// <param name="descriptionOfWhatThingIs"></param>
    /// <returns></returns>
    private static T2 GetBest<T2>(T2 explicitThing, T2 pipelineConfigurationThing, string descriptionOfWhatThingIs)
    {
        // if explicitThing and pipelineConfigurationThing are both null
        //Means: explicitThing == null && pipelineConfigurationThing == null
        if (EqualityComparer<T2>.Default.Equals(explicitThing, default) &&
            EqualityComparer<T2>.Default.Equals(pipelineConfigurationThing, default))
            throw new Exception(
                $"No explicit {descriptionOfWhatThingIs} was specified and there is no fixed {descriptionOfWhatThingIs} defined in the Pipeline configuration in the Catalogue");

        //if one of them only is null - XOR
        if (EqualityComparer<T2>.Default.Equals(explicitThing, default) ^
            EqualityComparer<T2>.Default.Equals(pipelineConfigurationThing, default))
            return EqualityComparer<T2>.Default.Equals(explicitThing, default)
                ? pipelineConfigurationThing
                : explicitThing; //return the not null one

        //both of them are populated
        throw new Exception(
            $"Cannot have both the explicit {descriptionOfWhatThingIs} '{explicitThing}' (the code creating the pipeline said it had a specific {descriptionOfWhatThingIs} it wants to use) as well as the {descriptionOfWhatThingIs} configured in the Pipeline in the Catalogue '{pipelineConfigurationThing}' (this should have been picked up by the DataFlowPipelineContext checks above)");
    }


    /// <summary>
    /// Attempts to construct an instance of the class described by <see cref="IPipelineComponent.Class"/> and fulfil its <see cref="DemandsInitializationAttribute"/>.
    /// Returns null and populates <paramref name="ex"/> if this is not possible/errors.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static object TryCreateComponent(IPipelineComponent component, out Exception ex)
    {
        ex = null;
        try
        {
            return CreateComponent(component);
        }
        catch (Exception e)
        {
            ex = e;
            return null;
        }
    }

    private static object CreateComponent(IPipelineComponent toBuild)
    {
        var type = toBuild.GetClassAsSystemType() ?? throw new Exception($"Could not find Type '{toBuild.Class}'");
        var toReturn = ObjectConstructor.Construct(type);

        //all the IArguments we need to initialize the class
        var allArguments = toBuild.GetAllArguments().ToArray();

        //get all possible properties that we could set on the underlying class
        foreach (var propertyInfo in toReturn.GetType().GetProperties())
        {
            SetPropertyIfDemanded(toBuild, toReturn, propertyInfo, allArguments);

            //see if any demand nested initialization
            var nestedInit =
                Attribute.GetCustomAttributes(propertyInfo)
                    .FirstOrDefault(a => a is DemandsNestedInitializationAttribute);

            //this one does
            if (nestedInit != null)
            {
                // initialise the container before nesting-setting all properties
                var container = Activator.CreateInstance(propertyInfo.PropertyType);

                foreach (var nestedProp in propertyInfo.PropertyType.GetProperties())
                    SetPropertyIfDemanded(toBuild, container, nestedProp, allArguments, propertyInfo);

                //use reflection to set the container
                propertyInfo.SetValue(toReturn, container, null);
            }
        }

        return toReturn;
    }

    /// <summary>
    /// Sets the value of a property on instance toReturn.
    /// </summary>
    /// <param name="toBuild">IPipelineComponent which is the persistence record - the template of what to build</param>
    /// <param name="toReturn">An instance of the Class referenced by IPipelineComponent.Class (or in the case of [DemandsNestedInitializationAttribute] a reference to the nested property)</param>
    /// <param name="propertyInfo">The specific property you are trying to populate on toBuild</param>
    /// <param name="arguments">IArguments of toBuild (the values to populate toReturn with)</param>
    /// <param name="nestedProperty">If you are populating a sub property of the class then pass the instance of the sub property as toBuild and pass the nesting property as nestedProperty</param>
    private static void SetPropertyIfDemanded(IPipelineComponent toBuild, object toReturn, PropertyInfo propertyInfo,
        IArgument[] arguments, PropertyInfo nestedProperty = null)
    {
        //see if any demand initialization
        var initialization =
            (DemandsInitializationAttribute)
            Attribute.GetCustomAttributes(propertyInfo)
                .FirstOrDefault(a => a is DemandsInitializationAttribute);

        //this one does
        if (initialization != null)
            try
            {
                //look for 'DeleteUsers' if not nested
                //look for 'Settings.DeleteUsers' if nested in a property called Settings on class
                var expectedArgumentName = nestedProperty != null
                    ? $"{nestedProperty.Name}.{propertyInfo.Name}"
                    : propertyInfo.Name;

                //get the appropriate value from arguments
                var argument = arguments.SingleOrDefault(n => n.Name.Equals(expectedArgumentName));

                //if there is no matching argument and no default value
                if (argument == null)
                    if (initialization.DefaultValue == null && initialization.Mandatory)
                    {
                        var msg =
                            $"Class {toReturn.GetType().Name} has a property {propertyInfo.Name} marked with DemandsInitialization but no corresponding argument was found in the arguments (PipelineComponentArgument) of the PipelineComponent called {toBuild.Name}";

                        throw new PropertyDemandNotMetException(msg, toBuild, propertyInfo);
                    }
                    else
                    {
                        //use reflection to set the value
                        propertyInfo.SetValue(toReturn, initialization.DefaultValue, null);
                    }
                else
                    //use reflection to set the value
                    propertyInfo.SetValue(toReturn, argument.GetValueAsSystemType(), null);
            }
            catch (NotSupportedException e)
            {
                throw new Exception(
                    $"Class {toReturn.GetType().Name} has a property {propertyInfo.Name} but is of unexpected/unsupported type {propertyInfo.GetType()}",
                    e);
            }
    }

    /// <summary>
    /// Retrieves and creates an instance of the class described in the blueprint <see cref="IPipeline.Source"/> if there is one.  Pipelines do not have
    /// to have a source if the use case requires a fixed source instance generated at runtime.
    /// </summary>
    /// <param name="pipeline"></param>
    /// <returns></returns>
    public static object CreateSourceIfExists(IPipeline pipeline)
    {
        var source = pipeline.Source;

        //there is no configured destination
        return source == null ? null : CreateComponent(source);
    }

    /// <summary>
    /// Retrieves and creates an instance of the class described in the blueprint <see cref="IPipeline.Destination"/> if there is one.  Pipelines do not have
    /// to have a destination if the use case requires a fixed destination instance generated at runtime
    /// </summary>
    public static object CreateDestinationIfExists(IPipeline pipeline)
    {
        var destination = pipeline.Destination;

        //there is no configured destination
        if (destination == null)
            return null;

        //throw new NotSupportedException("The IsDestination PipelineComponent of pipeline '" + pipeline.Name + "' is an IDataFlowComponent but it is not an IDataFlowDestination which is a requirement of all destinations");

        return CreateComponent(destination);
    }

    /// <summary>
    /// Attempts to create an instance of <see cref="IDataFlowPipelineEngine"/> described by the blueprint <paramref name="pipeline"/>.  Components are then checked if they
    /// support <see cref="ICheckable"/> using the <paramref name="checkNotifier"/> to record the results.
    /// </summary>
    /// <param name="pipeline">The blueprint to attempt to generate</param>
    /// <param name="checkNotifier">The event notifier to record how it went</param>
    /// <param name="initizationObjects">The objects available for fulfilling IPipelineRequirements</param>
    public void Check(IPipeline pipeline, ICheckNotifier checkNotifier, object[] initizationObjects)
    {
        //Try to construct the pipeline into an in memory Engine based on the in Catalogue blueprint (Pipeline)
        IDataFlowPipelineEngine dataFlowPipelineEngine = null;
        try
        {
            dataFlowPipelineEngine = Create(pipeline, new FromCheckNotifierToDataLoadEventListener(checkNotifier));
            checkNotifier.OnCheckPerformed(new CheckEventArgs("Pipeline successfully constructed in memory",
                CheckResult.Success));
        }
        catch (Exception exception)
        {
            checkNotifier.OnCheckPerformed(
                new CheckEventArgs("Failed to construct pipeline, see Exception for details", CheckResult.Fail,
                    exception));
        }

        if (initizationObjects == null)
        {
            checkNotifier.OnCheckPerformed(new CheckEventArgs(
                "initializationObjects parameter has not been set (this is a programmer error most likely ask your developer to fix it - this parameter should be empty not null)",
                CheckResult.Fail));
            return;
        }

        //Initialize each component with the initialization objects so that they can check themselves (note that this should be preview data, hopefully the components don't run off and start nuking stuff just because they got their GO objects)
        if (dataFlowPipelineEngine != null)
        {
            try
            {
                dataFlowPipelineEngine.Initialize(initizationObjects);
                checkNotifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Pipeline successfully initialized with {initizationObjects.Length} initialization objects",
                        CheckResult.Success));
            }
            catch (Exception exception)
            {
                checkNotifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Pipeline initialization failed, there were {initizationObjects.Length} objects for use in initialization ({string.Join(",", initizationObjects.Select(o => o.ToString()))})",
                        CheckResult.Fail,
                        exception));
            }

            checkNotifier.OnCheckPerformed(new CheckEventArgs("About to check engine/components", CheckResult.Success));
            dataFlowPipelineEngine.Check(checkNotifier);
        }
    }
}