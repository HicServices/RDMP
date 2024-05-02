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
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Often an ExtractionFilter will have a parameter associated with it (or more than one).  In this case it can be that
///     you want to curate various values and give them
///     meaningful titles.  For example if you have a filter 'Hospitalised with condition X' which has parameter
///     @ConditionList.  Then you decide that you want to curate
///     a list 'A101.23,B21.1' as 'People hospitalised with drug dependency'.  This 'known meaningful parameter values set'
///     is called a ExtractionFilterParameterSet.  You
///     can provide a name and a description for the concept.  Then you create a value for each parameter in the associated
///     filter.  See ExtractionFilterParameterSetValue for
///     the value recordings.
/// </summary>
public class ExtractionFilterParameterSet : DatabaseEntity, ICollectSqlParameters, INamed
{
    #region Database Properties

    private string _name;
    private string _description;
    private int _extractionFilterID;

    /// <inheritdoc />
    [NotNull]
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    ///     Human-readable description of what the parameter set identifies e.g. 'Diabetes Drugs' and any supporting
    ///     information about how it works, quirks etc
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>
    ///     The filter which the parameter values are designed to work with
    /// </summary>
    public int ExtractionFilter_ID
    {
        get => _extractionFilterID;
        set => SetField(ref _extractionFilterID, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="ExtractionFilter_ID" />
    [NoMappingToDatabase]
    public ExtractionFilter ExtractionFilter => Repository.GetObjectByID<ExtractionFilter>(ExtractionFilter_ID);

    /// <summary>
    ///     Gets all the individual parameter values required for populating the filter to achieve this concept (e.g. 'Diabetes
    ///     Drugs' might have 2 parameter values @DrugList='123.122.1,1.2... etc' and @DrugCodeFormat='bnf')
    /// </summary>
    [NoMappingToDatabase]
    public IEnumerable<ExtractionFilterParameterSetValue> Values =>
        Repository.GetAllObjectsWithParent<ExtractionFilterParameterSetValue>(this);

    #endregion

    public ExtractionFilterParameterSet()
    {
    }

    internal ExtractionFilterParameterSet(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Name = r["Name"].ToString();
        Description = r["Description"] as string;
        ExtractionFilter_ID = Convert.ToInt32(r["ExtractionFilter_ID"]);
    }

    /// <summary>
    ///     Defines a new set of known parameter values to achieve a given goal (e.g. identify 'diabetic drugs' in dataset
    ///     prescriptions) in combination with a parent <see cref="IFilter" />.
    ///     <para>
    ///         A single <see cref="ExtractionFilter" /> (e.g. 'Drug Prescriptions of X' with parameter @DrugList) could have
    ///         many <see cref="ExtractionFilterParameterSet" />
    ///     </para>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="filter"></param>
    /// <param name="name"></param>
    public ExtractionFilterParameterSet(ICatalogueRepository repository, ExtractionFilter filter, string name = null)
    {
        name ??= $"New ExtractionFilterParameterSet {Guid.NewGuid()}";

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "ExtractionFilter_ID", filter.ID }
        });
    }


    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    /// <inheritdoc cref="Values" />
    public ISqlParameter[] GetAllParameters()
    {
        return Values.ToArray();
    }

    /// <summary>
    ///     Identifies all parameters which do not exist yet as declared values
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ExtractionFilterParameter> GetMissingEntries()
    {
        var existingCatalogueParameters =
            ExtractionFilter.GetAllParameters().Cast<ExtractionFilterParameter>().ToArray();

        var personalChildren = Values.ToArray();

        foreach (var catalogueParameter in existingCatalogueParameters)
            if (personalChildren.All(c => c.ExtractionFilterParameter_ID != catalogueParameter.ID))
                yield return catalogueParameter;
    }

    /// <summary>
    ///     Creates new value entries for each parameter in the filter that does not yet have a value in this value set
    /// </summary>
    /// <returns></returns>
    public ExtractionFilterParameterSetValue[] CreateNewValueEntries()
    {
        var toReturn = new List<ExtractionFilterParameterSetValue>();

        foreach (var catalogueParameter in GetMissingEntries())
            //we have a master that does not have any child values yet
            toReturn.Add(new ExtractionFilterParameterSetValue((ICatalogueRepository)Repository, this,
                catalogueParameter));

        return toReturn.ToArray();
    }

    public override void DeleteInDatabase()
    {
        foreach (var v in Values) v.DeleteInDatabase();

        base.DeleteInDatabase();
    }
}