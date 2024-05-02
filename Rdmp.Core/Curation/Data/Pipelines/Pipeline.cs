// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.Curation.Data.Pipelines;

/// <inheritdoc cref="IPipeline" />
public class Pipeline : DatabaseEntity, IPipeline, IHasDependencies
{
    #region Database Properties

    private string _name;
    private string _description;
    private int? _destinationPipelineComponentID;
    private int? _sourcePipelineComponentID;

    /// <inheritdoc />
    [NotNull]
    [Unique]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc />
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <inheritdoc />
    public int? DestinationPipelineComponent_ID
    {
        get => _destinationPipelineComponentID;
        set => SetField(ref _destinationPipelineComponentID, value);
    }

    /// <inheritdoc />
    public int? SourcePipelineComponent_ID
    {
        get => _sourcePipelineComponentID;
        set => SetField(ref _sourcePipelineComponentID, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc />
    [NoMappingToDatabase]
    public IList<IPipelineComponent> PipelineComponents => _knownPipelineComponents.Value;

    /// <inheritdoc />
    [NoMappingToDatabase]
    public IPipelineComponent Destination
    {
        get
        {
            return DestinationPipelineComponent_ID == null
                ? null
                : _knownPipelineComponents.Value.Single(c => c.ID == DestinationPipelineComponent_ID.Value);
        }
    }

    /// <inheritdoc />
    [NoMappingToDatabase]
    public IPipelineComponent Source
    {
        get
        {
            return SourcePipelineComponent_ID == null
                ? null
                : _knownPipelineComponents.Value.Single(c => c.ID == SourcePipelineComponent_ID.Value);
        }
    }

    #endregion

    public Pipeline()
    {
        ClearAllInjections();
    }

    /// <summary>
    ///     Creates a new empty <see cref="Pipeline" /> in the database, this is a sequence of <see cref="PipelineComponent" />
    ///     which when combined
    ///     with an <see cref="IPipelineUseCase" /> achieve a specific goal (e.g. loading records into the database from a flat
    ///     file).
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="name"></param>
    public Pipeline(ICatalogueRepository repository, string name = null)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name ?? $"NewPipeline {Guid.NewGuid()}" }
        });

        ClearAllInjections();
    }

    internal Pipeline(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Name = r["Name"].ToString();

        var o = r["DestinationPipelineComponent_ID"];
        if (o == DBNull.Value)
            DestinationPipelineComponent_ID = null;
        else
            DestinationPipelineComponent_ID = Convert.ToInt32(o);

        o = r["SourcePipelineComponent_ID"];
        if (o == DBNull.Value)
            SourcePipelineComponent_ID = null;
        else
            SourcePipelineComponent_ID = Convert.ToInt32(o);

        Description = r["Description"] as string;

        ClearAllInjections();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    ///     Creates (in the database) and returns a new <see cref="Pipeline" /> which is an identical copy of the current.
    ///     This includes creating new copies
    ///     of all child objects (i.e. <see cref="PipelineComponent" /> and <see cref="PipelineComponentArgument" />)
    /// </summary>
    /// <returns></returns>
    public Pipeline Clone()
    {
        var name = GetUniqueCloneName();

        var clonePipe = new Pipeline((ICatalogueRepository)Repository, name)
        {
            Description = Description
        };

        var originalSource = Source;
        if (originalSource != null)
        {
            var cloneSource = originalSource.Clone(clonePipe);
            clonePipe.SourcePipelineComponent_ID = cloneSource.ID;
        }

        var originalDestination = Destination;
        if (originalDestination != null)
        {
            var cloneDestination = originalDestination.Clone(clonePipe);
            clonePipe.DestinationPipelineComponent_ID = cloneDestination.ID;
        }

        clonePipe.SaveToDatabase();

        foreach (var component in PipelineComponents)
        {
            //if the component is one of the ones we already cloned
            if (Equals(originalSource, component) || Equals(originalDestination, component))
                continue;

            component.Clone(clonePipe);
        }


        return clonePipe;
    }

    private string GetUniqueCloneName()
    {
        var otherPipelines = CatalogueRepository.GetAllObjects<Pipeline>();

        var proposedName = $"{Name} (Clone)";
        var suffix = 1;

        while (otherPipelines.Any(p => proposedName.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase)))
        {
            suffix++;
            proposedName = $"{Name} (Clone{suffix})";
        }

        return proposedName;
    }

    /// <inheritdoc />
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        return Array.Empty<IHasDependencies>();
    }

    /// <inheritdoc />
    public IHasDependencies[] GetObjectsDependingOnThis()
    {
        return PipelineComponents.Cast<IHasDependencies>().ToArray();
    }

    private Lazy<IList<IPipelineComponent>> _knownPipelineComponents;

    /// <inheritdoc />
    public void InjectKnown(IPipelineComponent[] instance)
    {
        _knownPipelineComponents = new Lazy<IList<IPipelineComponent>>(() => instance.OrderBy(p => p.Order).ToList());
    }

    /// <inheritdoc />
    public void ClearAllInjections()
    {
        _knownPipelineComponents = new Lazy<IList<IPipelineComponent>>(FetchPipelineComponents);
    }

    private IList<IPipelineComponent> FetchPipelineComponents()
    {
        return Repository.GetAllObjectsWithParent<PipelineComponent>(this)
            .Cast<IPipelineComponent>()
            .OrderBy(p => p.Order)
            .ToList();
    }
}