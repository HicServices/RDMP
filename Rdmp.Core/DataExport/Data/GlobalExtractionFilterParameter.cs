// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
///     Sometimes you want to define global parameters that apply to an entire ExtractionConfiguration and all the
///     Catalogues/ExtractableDataSets within it.  For example you might
///     want to define @studyStartWindow and @studyEndWindow as global parameters which can be used to restrict the
///     extraction window of each dataset.  GlobalExtractionFilterParameters
///     are created and assocaited with a single ExtractionConfiguration after which they are available for use in all
///     DeployedExtractionFilters of all datasets within the configuration.
///     <para>It also means you have a single point you can change the parameter if you need to adjust it later on.</para>
/// </summary>
public class GlobalExtractionFilterParameter : DatabaseEntity, ISqlParameter, IInjectKnown<IQuerySyntaxHelper>
{
    /// <inheritdoc />
    [NoMappingToDatabase]
    public string ParameterName => QuerySyntaxHelper.GetParameterNameFromDeclarationSQL(ParameterSQL);

    #region Database Properties

    private string _parameterSQL;
    private string _value;
    private string _comment;
    private int _extractionConfiguration_ID;

    /// <inheritdoc />
    [Sql]
    public string ParameterSQL
    {
        get => _parameterSQL;
        set => SetField(ref _parameterSQL, value);
    }

    /// <inheritdoc />
    public string Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    /// <inheritdoc />
    public string Comment
    {
        get => _comment;
        set => SetField(ref _comment, value);
    }

    /// <summary>
    ///     Which <see cref="ExtractionConfiguration" /> the parameter is declared on.  This parameter will be available for
    ///     referencing in any <see cref="ISelectedDataSets" /> which
    ///     are part of the configuration.
    /// </summary>
    public int ExtractionConfiguration_ID
    {
        get => _extractionConfiguration_ID;
        set => SetField(ref _extractionConfiguration_ID, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="ExtractionConfiguration_ID" />
    [NoMappingToDatabase]
    public ExtractionConfiguration ExtractionConfiguration =>
        Repository.GetObjectByID<ExtractionConfiguration>(ExtractionConfiguration_ID);

    #endregion

    public GlobalExtractionFilterParameter()
    {
        ClearAllInjections();
    }

    /// <summary>
    ///     Creates a new parameter into the <paramref name="repository" /> database acting as a global parameter for all
    ///     <see cref="ISelectedDataSets" /> in the <paramref name="configuration" />
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="configuration"></param>
    /// <param name="parameterSQL"></param>
    public GlobalExtractionFilterParameter(IDataExportRepository repository, ExtractionConfiguration configuration,
        string parameterSQL)
    {
        Repository = repository;

        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ParameterSQL", parameterSQL },
            { "ExtractionConfiguration_ID", configuration.ID }
        });
    }


    /// <summary>
    ///     Reads an existing instance out of the database
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="r"></param>
    internal GlobalExtractionFilterParameter(IDataExportRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Value = r["Value"] as string;
        ExtractionConfiguration_ID = (int)r["ExtractionConfiguration_ID"];
        ParameterSQL = r["ParameterSQL"] as string;
        Comment = r["Comment"] as string;
    }

    /// <summary>
    ///     Returns <see cref="ParameterName" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{ParameterName} = {Value}";
    }


    /// <inheritdoc />
    public void Check(ICheckNotifier notifier)
    {
        new ParameterSyntaxChecker(this).Check(notifier);
    }

    /// <inheritdoc />
    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        return _syntaxHelper ??
               throw new NotSupportedException(
                   "Global extraction parameters are multi database type (depending on which ExtractableDataSet they are being used with)");
    }

    /// <inheritdoc />
    public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
    {
        return ExtractionConfiguration;
    }

    private IQuerySyntaxHelper _syntaxHelper;

    /// <inheritdoc />
    public void InjectKnown(IQuerySyntaxHelper instance)
    {
        _syntaxHelper = instance;
    }

    /// <inheritdoc />
    public void ClearAllInjections()
    {
        _syntaxHelper = null;
    }
}