// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding.Parameters;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.FilterImporting;

public delegate ISqlParameter CreateNewSqlParameterHandler(ICollectSqlParameters collector, string parameterName);

/// <summary>
///     Models a <see cref="Collector" /> who has 0 or more <see cref="ISqlParameter" /> associated
///     with it (handled by a <see cref="ParameterManager" />).
/// </summary>
public class ParameterCollectionUIOptions
{
    public ICollectSqlParameters Collector { get; set; }

    /// <summary>
    ///     True if the <see cref="Collector" /> is <see cref="IMightBeReadOnly" /> and is readonly
    /// </summary>
    public bool ReadOnly { get; set; }

    public ParameterLevel CurrentLevel { get; set; }
    public ParameterManager ParameterManager { get; set; }
    private readonly CreateNewSqlParameterHandler _createNewParameterDelegate;

    public readonly ParameterRefactorer Refactorer = new();

    public string UseCase { get; private set; }

    public ParameterCollectionUIOptions(string useCase, ICollectSqlParameters collector, ParameterLevel currentLevel,
        ParameterManager parameterManager, CreateNewSqlParameterHandler createNewParameterDelegate = null)
    {
        UseCase = useCase;
        Collector = collector;
        CurrentLevel = currentLevel;
        ParameterManager = parameterManager;
        _createNewParameterDelegate = createNewParameterDelegate;

        if (_createNewParameterDelegate == null)
            if (AnyTableSqlParameter.IsSupportedType(collector.GetType()))
                _createNewParameterDelegate = CreateNewParameterDefaultImplementation;

        if (collector is IMightBeReadOnly ro)
            ReadOnly = ro.ShouldBeReadOnly(out _);
    }


    /// <summary>
    ///     Method called when creating new parameters if no CreateNewSqlParameterHandler was provided during construction
    /// </summary>
    /// <returns></returns>
    private ISqlParameter CreateNewParameterDefaultImplementation(ICollectSqlParameters collector, string parameterName)
    {
        if (!parameterName.StartsWith("@"))
            parameterName = $"@{parameterName}";

        var entity = (IMapsDirectlyToDatabaseTable)collector;
        var newParam = new AnyTableSqlParameter((ICatalogueRepository)entity.Repository, entity,
            AnyTableSqlParameter.GetDefaultDeclaration(parameterName))
        {
            Value = AnyTableSqlParameter.DefaultValue
        };
        newParam.SaveToDatabase();
        return newParam;
    }

    public bool CanNewParameters()
    {
        return _createNewParameterDelegate != null;
    }

    public ISqlParameter CreateNewParameter(string parameterName)
    {
        return _createNewParameterDelegate(Collector, parameterName);
    }

    public bool IsHigherLevel(ISqlParameter parameter)
    {
        return ParameterManager.GetLevelForParameter(parameter) > CurrentLevel;
    }

    private bool IsDifferentLevel(ISqlParameter p)
    {
        return ParameterManager.GetLevelForParameter(p) != CurrentLevel;
    }

    public bool IsOverridden(ISqlParameter sqlParameter)
    {
        return ParameterManager.GetOverrideIfAnyFor(sqlParameter) != null;
    }

    public bool ShouldBeDisabled(ISqlParameter p)
    {
        return IsOverridden(p) || IsHigherLevel(p) || p is SpontaneousObject;
    }

    public bool ShouldBeReadOnly(ISqlParameter p)
    {
        return ReadOnly || IsOverridden(p) || IsDifferentLevel(p) || p is SpontaneousObject;
    }
}