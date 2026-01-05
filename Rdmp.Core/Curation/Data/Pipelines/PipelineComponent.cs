// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Curation.Data.Pipelines;

/// <inheritdoc cref="IPipelineComponent"/>
public class PipelineComponent : DatabaseEntity, IPipelineComponent
{
    #region Database Properties

    private string _name;
    private int _order;
    private int _pipelineID;
    private string _class;

    /// <inheritdoc/>
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc/>
    public int Order
    {
        get => _order;
        set => SetField(ref _order, value);
    }

    /// <inheritdoc/>
    public int Pipeline_ID
    {
        get => _pipelineID;
        set => SetField(ref _pipelineID, value);
    }

    /// <inheritdoc/>
    public string Class
    {
        get => _class;
        set => SetField(ref _class, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public IEnumerable<IPipelineComponentArgument> PipelineComponentArguments =>
        Repository.GetAllObjectsWithParent<PipelineComponentArgument>(this);

    /// <inheritdoc cref="Pipeline_ID"/>
    [NoMappingToDatabase]
    public IHasDependencies Pipeline => Repository.GetObjectByID<Pipeline>(Pipeline_ID);

    #endregion

    public PipelineComponent()
    {
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>
    /// Creates a new component in the <paramref name="parent"/> <see cref="Pipeline"/>.  This will mean that when run the <see cref="Pipeline"/>
    /// will instantiate and run the given <paramref name="componentType"/>.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parent"></param>
    /// <param name="componentType"></param>
    /// <param name="order"></param>
    /// <param name="name"></param>
    public PipelineComponent(ICatalogueRepository repository, IPipeline parent, Type componentType, int order,
        string name = null)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name ?? $"Run {componentType.Name}" },
            { "Pipeline_ID", parent.ID },
            { "Class", componentType.ToString() },
            { "Order", order }
        });
    }

    internal PipelineComponent(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Order = int.Parse(r["Order"].ToString());
        Pipeline_ID = int.Parse(r["Pipeline_ID"].ToString());
        Class = r["Class"].ToString();
        Name = r["Name"].ToString();
    }

    /// <inheritdoc/>
    public IEnumerable<IArgument> GetAllArguments() => PipelineComponentArguments;

    /// <inheritdoc/>
    public IArgument CreateNewArgument() => new PipelineComponentArgument((ICatalogueRepository)Repository, this);

    /// <inheritdoc/>
    public string GetClassNameWhoArgumentsAreFor() => Class;

    /// <inheritdoc/>
    public Type GetClassAsSystemType() => MEF.GetType(Class);

    /// <inheritdoc/>
    public string GetClassNameLastPart() =>
        string.IsNullOrWhiteSpace(Class) ? Class : Class[(Class.LastIndexOf('.') + 1)..];

    /// <inheritdoc/>
    public PipelineComponent Clone(Pipeline intoTargetPipeline)
    {
        var cataRepo = (ICatalogueRepository)intoTargetPipeline.Repository;

        var type = GetClassAsSystemType();

        var clone = new PipelineComponent(cataRepo, intoTargetPipeline, type ?? typeof(object), Order);

        // the Type for the PipelineComponent could not be resolved
        // Maybe the user created this pipe with a Plugin and then uninstalled
        // the plugin.  So tell the API its an Object then update the Class
        // to the name of it even though it doesnt exist (its just cloning after all)

        if (type == null)
            clone.Class = Class;

        foreach (var argument in PipelineComponentArguments) argument.Clone(clone);

        clone.Name = Name;
        clone.SaveToDatabase();

        return clone;
    }

    /// <inheritdoc/>
    public IArgument[] CreateArgumentsForClassIfNotExists<T>() => CreateArgumentsForClassIfNotExists(typeof(T));

    /// <inheritdoc/>
    public IArgument[] CreateArgumentsForClassIfNotExists(Type underlyingComponentType)
    {
        var argFactory = new ArgumentFactory();
        return ArgumentFactory.CreateArgumentsForClassIfNotExistsGeneric(underlyingComponentType,

                //tell it how to create new instances of us related to parent
                this,

                //what arguments already exist
                PipelineComponentArguments.ToArray())

            //convert the result back from generic to specific (us)
            .Cast<PipelineComponentArgument>().ToArray();
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        return new IHasDependencies[] { Pipeline };
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsDependingOnThis() =>
        PipelineComponentArguments.Cast<IHasDependencies>().ToArray();

    public override void DeleteInDatabase()
    {
        if (Pipeline is Pipeline parent)
        {
            if (parent.SourcePipelineComponent_ID == ID)
                CatalogueRepository.SaveSpecificPropertyOnlyToDatabase(parent, "SourcePipelineComponent_ID", null);

            if (parent.DestinationPipelineComponent_ID == ID)
                CatalogueRepository.SaveSpecificPropertyOnlyToDatabase(parent, "DestinationPipelineComponent_ID", null);
        }

        base.DeleteInDatabase();
    }

    private static bool IsGenericType(Type toCheck, Type genericType)
    {
        return toCheck.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericType);
    }

    public static PipelineComponentRole GetRoleFor(Type componentType)
    {
        if (IsGenericType(componentType, typeof(IDataFlowSource<>)))
            return PipelineComponentRole.Source;

        if (IsGenericType(componentType, typeof(IDataFlowDestination<>)))
            return PipelineComponentRole.Destination;

        return IsGenericType(componentType, typeof(IDataFlowComponent<>))
            ? PipelineComponentRole.Middle
            : throw new ArgumentException($"Object must be an IDataFlowComponent<> but was {componentType}");
    }
}