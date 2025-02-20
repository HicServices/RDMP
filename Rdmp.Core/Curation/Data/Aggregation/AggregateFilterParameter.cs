// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Data.Common;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data.Aggregation;

/// <summary>
/// Each AggregateFilter can have 1 or more AggregateFilterParameters, these allows you to specify an SQL parameter that the user can adjust at runtime to change
/// how a given filter works.  E.g. if you have a filter 'Prescribed after @startDate' you would have an AggregateFilterParameter called @startDate with an appropriate
/// user friendly description.
/// </summary>
public class AggregateFilterParameter : DatabaseEntity, ISqlParameter
{
    #region Database Properties

    private int _aggregateFilterID;
    private string _parameterSQL;
    private string _value;
    private string _comment;

    /// <summary>
    /// The ID of the <see cref="AggregateFilter"/> to which this parameter should be used with.  The filter should have a reference to the parameter name (e.g. @startDate)
    /// in its WhereSQL.
    /// </summary>
    [Relationship(typeof(AggregateFilter), RelationshipType.SharedObject)]
    public int AggregateFilter_ID
    {
        get => _aggregateFilterID;
        set => SetField(ref _aggregateFilterID, value);
    } // changing this is required for cloning functionality i.e. clone parameter then point it to new parent


    /// <inheritdoc/>
    [Sql]
    public string ParameterSQL
    {
        get => _parameterSQL;
        set => SetField(ref _parameterSQL, value);
    }

    /// <inheritdoc/>
    [Sql]
    public string Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    /// <inheritdoc/>
    public string Comment
    {
        get => _comment;
        set => SetField(ref _comment, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="AggregateFilter_ID"/>
    [NoMappingToDatabase]
    public AggregateFilter AggregateFilter => Repository.GetObjectByID<AggregateFilter>(AggregateFilter_ID);

    #endregion

    /// <summary>
    /// extracts the name of the parameter from the SQL
    /// </summary>
    [NoMappingToDatabase]
    public string ParameterName => QuerySyntaxHelper.GetParameterNameFromDeclarationSQL(ParameterSQL);

    public AggregateFilterParameter()
    {
    }

    /// <summary>
    /// Declares a new parameter to be used by the specified AggregateFilter.  Use AggregateFilterFactory to call this
    /// constructor.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parameterSQL"></param>
    /// <param name="parent"></param>
    internal AggregateFilterParameter(ICatalogueRepository repository, string parameterSQL, AggregateFilter parent)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ParameterSQL", parameterSQL },
            { "AggregateFilter_ID", parent.ID }
        });
    }


    internal AggregateFilterParameter(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        AggregateFilter_ID = int.Parse(r["AggregateFilter_ID"].ToString());
        ParameterSQL = r["ParameterSQL"] as string;
        Value = r["Value"] as string;
        Comment = r["Comment"] as string;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{ParameterName} = {Value}";

    /// <inheritdoc cref="ParameterSyntaxChecker"/>
    public void Check(ICheckNotifier notifier)
    {
        new ParameterSyntaxChecker(this).Check(notifier);
    }

    /// <inheritdoc/>
    public IQuerySyntaxHelper GetQuerySyntaxHelper() => AggregateFilter.GetQuerySyntaxHelper();

    /// <inheritdoc/>
    public IMapsDirectlyToDatabaseTable GetOwnerIfAny() => AggregateFilter;


    public AggregateFilterParameter ShallowClone(AggregateFilter into)
    {
        var clone = new AggregateFilterParameter(CatalogueRepository, ParameterSQL, into);
        CopyShallowValuesTo(clone);
        return clone;
    }
}