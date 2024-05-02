// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Providers.Nodes.PipelineNodes;

/// <summary>
///     This class is a wrapper for a <see cref="Pipeline" /> that has been found to be compatible with a given
///     <see cref="PipelineUseCase" /> (in terms of the source /
///     destination components and flow type etc).
///     <para>
///         It is <see cref="SpontaneousObject" /> only so it appears under Ctrl+F window... not a pattern we want to
///         repeat.
///     </para>
/// </summary>
public class PipelineCompatibleWithUseCaseNode : SpontaneousObject, IMasqueradeAs
{
    public Pipeline Pipeline { get; }
    public PipelineUseCase UseCase { get; }
    private readonly Type _useCaseType;

    public PipelineCompatibleWithUseCaseNode(MemoryRepository repo, Pipeline pipeline, PipelineUseCase useCase) :
        base(null)
    {
        Pipeline = pipeline;
        UseCase = useCase;
        Repository = repo;
        _useCaseType = UseCase.GetType();
    }

    public object MasqueradingAs()
    {
        return Pipeline;
    }

    public override string ToString()
    {
        return Pipeline.Name;
    }

    public override void DeleteInDatabase()
    {
        Pipeline.DeleteInDatabase();
    }

    public override bool Exists()
    {
        return Pipeline.Exists();
    }

    #region Equality

    protected bool Equals(PipelineCompatibleWithUseCaseNode other)
    {
        return _useCaseType == other._useCaseType && Pipeline.Equals(other.Pipeline);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PipelineCompatibleWithUseCaseNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_useCaseType, Pipeline);
    }

    #endregion
}