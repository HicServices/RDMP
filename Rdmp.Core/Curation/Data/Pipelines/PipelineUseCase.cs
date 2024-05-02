// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Curation.Data.Pipelines;

/// <summary>
///     Abstract base IPipelineUseCase. Provides basic implementations for filtering compatible pipelines and translating
///     a selected IPipeline into an actual executable engine instance via DataFlowPipelineEngineFactory.  Set
///     ExplicitSource /
///     ExplicitDestination / PreInitialize objects etc as needed for your use case.
/// </summary>
public abstract class PipelineUseCase : IPipelineUseCase
{
    /// <inheritdoc />
    public HashSet<object> GetInitializationObjects()
    {
        return InitializationObjects;
    }

    /// <inheritdoc />
    public IDataFlowPipelineContext GetContext()
    {
        return _context ?? throw new Exception(
            $"Context has not been initialized yet for use case {GetType()} make sure to add a call to GenerateContext method in the constructor (and mark class as sealed)");
    }

    /// <summary>
    ///     Call this in your constructor
    /// </summary>
    protected void GenerateContext()
    {
        _context = GenerateContextImpl();
    }

    /// <summary>
    ///     Implement this to generate the compatiblity definition for pipelines that will be used by you.
    ///     <para>IMPORTANT: Make sure you call <see cref="GenerateContext" /> in every constructor you have</para>
    /// </summary>
    /// <returns></returns>
    protected abstract IDataFlowPipelineContext GenerateContextImpl();

    /// <inheritdoc />
    public object ExplicitSource { get; protected set; }

    /// <inheritdoc />
    public object ExplicitDestination { get; protected set; }

    /// <summary>
    ///     True if there there are no objects available for hydrating (e.g. no files to load, no picked cohorts etc).  This is
    ///     often
    ///     the case when the user is editing a <see cref="Pipeline" /> at some arbitrary time.
    ///     <para>
    ///         If this is true then GetInitializationObjects should return Type[] instead of the actually selected objects
    ///         for the task
    ///     </para>
    /// </summary>
    public bool IsDesignTime { get; }

    protected HashSet<object> InitializationObjects = new();
    private IDataFlowPipelineContext _context;

    /// <summary>
    ///     The normal (non desing time) constructor.  Add your objects to <see cref="InitializationObjects" />
    /// </summary>
    protected PipelineUseCase()
    {
        IsDesignTime = false;
    }

    /// <summary>
    ///     Use this constructor if you are intending to use the <see cref="PipelineUseCase" /> for design time activities only
    ///     (building pipelines
    ///     from compatible components).  Only use if you don't have all the normally required object instances to actually
    ///     execute a pipeline.
    /// </summary>
    /// <param name="designTimeInitializationObjectTypes"></param>
    protected PipelineUseCase(Type[] designTimeInitializationObjectTypes)
    {
        IsDesignTime = true;
        foreach (var t in designTimeInitializationObjectTypes)
            InitializationObjects.Add(t);
    }

    /// <inheritdoc />
    public virtual IEnumerable<Pipeline> FilterCompatiblePipelines(IEnumerable<Pipeline> pipelines)
    {
        return pipelines.Where(IsAllowable);
    }

    /// <inheritdoc />
    public virtual IDataFlowPipelineEngine GetEngine(IPipeline pipeline, IDataLoadEventListener listener)
    {
        var engine = new DataFlowPipelineEngineFactory(this, pipeline).Create(pipeline, listener);
        engine.Initialize(GetInitializationObjects().ToArray());

        return engine;
    }

    /// <summary>
    ///     Mark the object instance <paramref name="o" /> as available for pipeline components to subscribe to via
    ///     IPipelineRequirement.
    /// </summary>
    /// <param name="o"></param>
    protected void AddInitializationObject(object o)
    {
        if (o != null)
            InitializationObjects.Add(o);
    }

    public bool IsAllowable(Pipeline pipeline)
    {
        // Pipeline is not compatible with the execution context of the pipeline use case
        if (!_context.IsAllowable(pipeline)) return false;
        try
        {
            // Does the pipeline contain any components that are invalid under the current list of available initialization objects etc
            foreach (var component in pipeline.PipelineComponents)
            {
                var type = component.GetClassAsSystemType();
                if (type == null) return false;

                var advert = new AdvertisedPipelineComponentTypeUnderContext(type, this);
                if (!advert.IsCompatible()) return false;
            }
        }
        catch (Exception)
        {
            // if any Pipeline components are broken e.g. not possible to find the Type they reference
            // then we tell the user it is not compatible
            return false;
        }

        // nothing incompatible here
        return true;
    }
}