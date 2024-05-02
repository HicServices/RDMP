// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Data.Common;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Describes an SQL parameter (e.g. @drugname) which is required for use of an <see cref="ExtractionFilter" /> (its
///     parent).
/// </summary>
public class ExtractionFilterParameter : DatabaseEntity, IDeleteable, ISqlParameter, IHasDependencies
{
    #region Database Properties

    private string _value;
    private string _comment;
    private string _parameterSQL;
    private int _extractionFilterID;

    /// <inheritdoc />
    [Sql]
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

    /// <inheritdoc />
    [Sql]
    public string ParameterSQL
    {
        get => _parameterSQL;
        set => SetField(ref _parameterSQL, value);
    }

    /// <summary>
    ///     The filter which requires this parameter belongs e.g. an <see cref="ExtractionFilter" />'Healthboard X' could have
    ///     a required property (<see cref="ExtractionFilterParameter" />) @Hb
    /// </summary>
    [Relationship(typeof(ExtractionFilter), RelationshipType.LocalReference)]
    public int ExtractionFilter_ID
    {
        get => _extractionFilterID;
        set => SetField(ref _extractionFilterID, value);
    }

    #endregion


    /// <summary>
    ///     extracts the name ofthe parameter from the SQL
    /// </summary>
    [NoMappingToDatabase]
    public string ParameterName => QuerySyntaxHelper.GetParameterNameFromDeclarationSQL(ParameterSQL);

    #region Relationships

    /// <inheritdoc cref="ExtractionFilter_ID" />
    [NoMappingToDatabase]
    public ExtractionFilter ExtractionFilter => Repository.GetObjectByID<ExtractionFilter>(ExtractionFilter_ID);

    #endregion

    public ExtractionFilterParameter()
    {
    }

    /// <summary>
    ///     Creates a new parameter on the given <paramref name="parent" />
    ///     <para>
    ///         It is better to use <see cref="ParameterCreator" /> to automatically generate parameters based on the WHERE
    ///         Sql
    ///     </para>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parameterSQL"></param>
    /// <param name="parent"></param>
    public ExtractionFilterParameter(ICatalogueRepository repository, string parameterSQL, ExtractionFilter parent)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ParameterSQL", parameterSQL },
            { "ExtractionFilter_ID", parent.ID }
        });
    }


    internal ExtractionFilterParameter(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        ExtractionFilter_ID = int.Parse(r["ExtractionFilter_ID"].ToString());
        ParameterSQL = r["ParameterSQL"] as string;
        Value = r["Value"] as string;
        Comment = r["Comment"] as string;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{ParameterName} = {Value}";
    }

    /// <inheritdoc cref="ParameterSyntaxChecker" />
    public void Check(ICheckNotifier notifier)
    {
        new ParameterSyntaxChecker(this).Check(notifier);
    }

    /// <inheritdoc />
    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        return ExtractionFilter.GetQuerySyntaxHelper();
    }

    /// <inheritdoc />
    public IHasDependencies[] GetObjectsThisDependsOn()
    {
        return new[] { ExtractionFilter };
    }

    /// <inheritdoc />
    public IHasDependencies[] GetObjectsDependingOnThis()
    {
        return null;
    }

    /// <summary>
    ///     Returns true if a  <see cref="Comment" /> has been provided and an example/initial <see cref="Value" /> specified.
    ///     This is a requirement of
    ///     publishing a filter as a master filter
    /// </summary>
    /// <param name="sqlParameter"></param>
    /// <param name="reasonParameterRejected"></param>
    /// <returns></returns>
    public static bool IsProperlyDocumented(ISqlParameter sqlParameter, out string reasonParameterRejected)
    {
        reasonParameterRejected = null;

        if (string.IsNullOrWhiteSpace(sqlParameter.ParameterSQL))
            reasonParameterRejected = "There is no ParameterSQL";
        else if (string.IsNullOrWhiteSpace(sqlParameter.Value))
            reasonParameterRejected = "There is no value/default value listed";


        return reasonParameterRejected == null;
    }

    /// <inheritdoc />
    public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
    {
        return ExtractionFilter;
    }
}