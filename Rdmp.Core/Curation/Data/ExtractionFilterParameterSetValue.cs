// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Stores a known specific useful value for a given ExtractionFilterParameter.  See
///     <see cref="Data.ExtractionFilterParameterSet" /> for a use case for this.
/// </summary>
public class ExtractionFilterParameterSetValue : DatabaseEntity, ISqlParameter, IInjectKnown<ExtractionFilterParameter>
{
    #region Database Properties

    private string _value;
    private int _extractionFilterParameterSetID;
    private int _extractionFilterParameterID;

    /// <summary>
    ///     The 'known good paramter set' (<see cref="ExtractionFilterParameterSet" />) to which this parameter value belongs
    /// </summary>
    public int ExtractionFilterParameterSet_ID
    {
        get => _extractionFilterParameterSetID;
        set => SetField(ref _extractionFilterParameterSetID, value);
    }

    /// <summary>
    ///     The specific parameter that this object is providing a 'known value' for in the parent
    ///     <see cref="ExtractionFilter" /> e.g. @DrugList='123.2,123.2,... etc'.
    /// </summary>
    public int ExtractionFilterParameter_ID
    {
        get => _extractionFilterParameterID;
        set => SetField(ref _extractionFilterParameterID, value);
    }

    /// <inheritdoc />
    [Sql]
    public string Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    #endregion

    #region cached values stored so we can act like a readonly ISqlParameter but secretly only Value will actually be changeable

    private Lazy<ExtractionFilterParameter> _knownExtractionFilterParameter;

    /// <inheritdoc />
    /// <remarks>Readonly, fetched from associated <see cref="ExtractionFilterParameter_ID" /></remarks>
    [NoMappingToDatabase]
    public string ParameterName => _knownExtractionFilterParameter.Value.ParameterName;

    /// <inheritdoc />
    /// <remarks>Readonly, fetched from associated <see cref="ExtractionFilterParameter_ID" /></remarks>
    [Sql]
    [NoMappingToDatabase]
    public string ParameterSQL
    {
        get => _knownExtractionFilterParameter.Value.ParameterSQL;
        set { }
    }

    /// <inheritdoc />
    /// <remarks>Readonly, fetched from associated <see cref="ExtractionFilterParameter_ID" /></remarks>
    [NoMappingToDatabase]
    public string Comment
    {
        get => _knownExtractionFilterParameter.Value.Comment;
        set { }
    }

    /// <summary>
    ///     Returns the <see cref="ExtractionFilterParameterSet" /> this known good value belongs to
    /// </summary>
    /// <returns></returns>
    public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
    {
        return ExtractionFilterParameterSet;
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="ExtractionFilterParameterSet_ID" />
    [NoMappingToDatabase]
    public ExtractionFilterParameterSet ExtractionFilterParameterSet =>
        Repository.GetObjectByID<ExtractionFilterParameterSet>(ExtractionFilterParameterSet_ID);

    /// <inheritdoc cref="ExtractionFilterParameter_ID" />
    [NoMappingToDatabase]
    public ExtractionFilterParameter ExtractionFilterParameter => _knownExtractionFilterParameter.Value;

    #endregion


    public ExtractionFilterParameterSetValue()
    {
        ClearAllInjections();
    }

    internal ExtractionFilterParameterSetValue(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        ExtractionFilterParameterSet_ID = Convert.ToInt32(r["ExtractionFilterParameterSet_ID"]);
        ExtractionFilterParameter_ID = Convert.ToInt32(r["ExtractionFilterParameter_ID"]);
        Value = r["Value"] as string;

        ClearAllInjections();
    }

    /// <summary>
    ///     Creates a record of what value to use with the given <see cref="ISqlParameter" /> of the
    ///     <see cref="ExtractionFilterParameterSet" /> <see cref="IFilter" /> to achieve the concept.
    ///     <para>
    ///         For example if there is an <see cref="ExtractionFilter" /> 'Prescribed Drug X' with a parameter @DrugList and
    ///         you create an <see cref="ExtractionFilterParameterSet" />
    ///         'Diabetic Drugs' then this will create a <see cref="ExtractionFilterParameterSetValue" /> of
    ///         '@DrugList='123.23,121,2... etc'.
    ///     </para>
    ///     <para>
    ///         If a filter has more than one parameter then you will need one
    ///         <see cref="ExtractionFilterParameterSetValue" /> per parameter per <see cref="ExtractionFilterParameterSet" />
    ///     </para>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parent"></param>
    /// <param name="valueIsForParameter"></param>
    public ExtractionFilterParameterSetValue(ICatalogueRepository repository, ExtractionFilterParameterSet parent,
        ExtractionFilterParameter valueIsForParameter)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ExtractionFilterParameterSet_ID", parent.ID },
            { "ExtractionFilterParameter_ID", valueIsForParameter.ID }
        });

        ClearAllInjections();
    }

    /// <inheritdoc />
    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        return ExtractionFilterParameter.GetQuerySyntaxHelper();
    }

    /// <inheritdoc />
    public void Check(ICheckNotifier notifier)
    {
        new ParameterSyntaxChecker(this).Check(notifier);
    }

    public override string ToString()
    {
        return $"{ParameterName} = {Value}";
    }

    public void InjectKnown(ExtractionFilterParameter instance)
    {
        _knownExtractionFilterParameter = new Lazy<ExtractionFilterParameter>(instance);
    }

    public void ClearAllInjections()
    {
        _knownExtractionFilterParameter = new Lazy<ExtractionFilterParameter>(() =>
            Repository.GetObjectByID<ExtractionFilterParameter>(ExtractionFilterParameter_ID));
    }
}