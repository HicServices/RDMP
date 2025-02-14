// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Curation.Data.Pipelines;

/// <summary>
/// Each PipelineComponent can have 0 or more PipelineComponentArguments, these function exactly like the relationship between ProcessTask and ProcessTaskArgument and
/// reflect a [DemandsInitialization] property on a class of type IDataFlowComponent which is built and populated by reflection from the PipelineComponent (serialization)
/// 
/// <para>See Pipeline and PipelineComponent for more information about this</para>
/// </summary>
public class PipelineComponentArgument : Argument, IPipelineComponentArgument
{
    #region Database Properties

    private int _pipelineComponentID;

    /// <inheritdoc/>
    public int PipelineComponent_ID
    {
        get => _pipelineComponentID;
        set => SetField(ref _pipelineComponentID, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="PipelineComponent_ID"/>
    [NoMappingToDatabase]
    public IPipelineComponent PipelineComponent => Repository.GetObjectByID<PipelineComponent>(PipelineComponent_ID);

    #endregion

    public PipelineComponentArgument()
    {
    }

    /// <summary>
    /// Creates a new argument storage object for one of the arguments in <see cref="PipelineComponent"/>.
    /// 
    /// <para>You should probably call <see cref="IArgumentHost.CreateArgumentsForClassIfNotExists{T}"/> instead</para>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parent"></param>
    public PipelineComponentArgument(ICatalogueRepository repository, PipelineComponent parent)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "PipelineComponent_ID", parent.ID },
            { "Name", $"Parameter{Guid.NewGuid()}" },
            { "Type", typeof(string).ToString() }
        });
    }

    internal PipelineComponentArgument(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        PipelineComponent_ID = int.Parse(r["PipelineComponent_ID"].ToString());
        Type = r["Type"].ToString();
        Name = r["Name"].ToString();
        Value = r["Value"] as string;
        Description = r["Description"] as string;
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        return new[] { PipelineComponent };
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsDependingOnThis() => Array.Empty<IHasDependencies>();

    /// <inheritdoc/>
    public void Clone(PipelineComponent intoTargetComponent)
    {
        var cloneArg = new PipelineComponentArgument(intoTargetComponent.CatalogueRepository, intoTargetComponent)
        {
            Name = Name,
            Value = Value,
            Type = Type,
            Description = Description
        };

        cloneArg.SaveToDatabase();
    }
}