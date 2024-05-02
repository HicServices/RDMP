// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Creates a new Lookup relationship between two <see cref="TableInfo" />.  This will allow you to optionally extract
///     code descriptions side by side code values (e.g. SexCode, SexCode_Desc)
///     by joining the two tables.  It also allows you to extract the <see cref="Lookup" /> <see cref="TableInfo" /> along
///     side the main dataset when it is extracted for research projects.
/// </summary>
public class ExecuteCommandCreateLookup : BasicCommandExecution
{
    private readonly ICatalogueRepository _catalogueRepository;
    private readonly ExtractionInformation _foreignKeyExtractionInformation;
    private readonly ColumnInfo[] _lookupDescriptionColumns;
    private readonly List<Tuple<ColumnInfo, ColumnInfo>> _fkToPkTuples;
    private readonly string _collation;
    private readonly bool _alsoCreateExtractionInformations;
    private readonly Catalogue _catalogue;
    private readonly ExtractionInformation[] _allExtractionInformations;

    /// <summary>
    ///     Creates a knowledge that one <see cref="TableInfo" /> provides a description for a code in a column of a
    ///     <see cref="Catalogue" /> (<see cref="ExtractionInformation" />).
    ///     There can be multiple join keys and multiple descriptions can be selected at once (e.g. SendingLocationCode=>
    ///     LocationTable.AddressLine1, LocationTable.AddressLine2) etc.
    ///     <para>See parameter descriptions for help</para>
    /// </summary>
    /// <param name="catalogueRepository"></param>
    /// <param name="foreignKeyExtractionInformation">
    ///     The extractable column in the main dataset which contains the lookup code
    ///     foreign key e.g. PatientSexCode
    /// </param>
    /// <param name="lookupDescriptionColumns">
    ///     The column(s) in the lookup that contain the free text description of the code
    ///     e.g. SexDescription, SexDescriptionLong etc
    /// </param>
    /// <param name="fkToPkTuples">
    ///     Must have at least 1, Item1 must be the foreign key column in the main dataset, Item2 must be the primary key
    ///     column in the lookup.
    ///     <para>
    ///         MOST lookups have 1 column paring only e.g. genderCode but some crazy lookups have duplication of code with
    ///         another column e.g. TestCode+Healthboard as primary keys into lookup
    ///     </para>
    /// </param>
    /// <param name="collation"></param>
    /// <param name="alsoCreateExtractionInformations">
    ///     True to create a new virtual column in the main dataset so that the code description appears inline with the rest
    ///     of
    ///     the column(s) in the dataset (when the SELECT query is built)
    /// </param>
    public ExecuteCommandCreateLookup(ICatalogueRepository catalogueRepository,
        ExtractionInformation foreignKeyExtractionInformation, ColumnInfo[] lookupDescriptionColumns,
        List<Tuple<ColumnInfo, ColumnInfo>> fkToPkTuples, string collation, bool alsoCreateExtractionInformations)
    {
        _catalogueRepository = catalogueRepository;
        _foreignKeyExtractionInformation = foreignKeyExtractionInformation;
        _lookupDescriptionColumns = lookupDescriptionColumns;
        _fkToPkTuples = fkToPkTuples;
        _collation = collation;
        _alsoCreateExtractionInformations = alsoCreateExtractionInformations;

        _catalogue = _foreignKeyExtractionInformation.CatalogueItem.Catalogue;
        _allExtractionInformations = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any);
        if (!_fkToPkTuples.Any())
            throw new Exception("You must pass at least 1 pair of keys");

        if (_fkToPkTuples.Any(t => t.Item1 == null || t.Item2 == null))
            throw new Exception("Tuples list contained null entries");
    }

    /// <summary>
    ///     Creates a knowledge that one <see cref="TableInfo" /> provides a description for a code in a column of a
    ///     <see cref="Catalogue" /> (<see cref="ExtractionInformation" />).
    /// </summary>
    /// <param name="catalogueRepository"></param>
    /// <param name="foreignKeyExtractionInformation">
    ///     The extractable column in the main dataset which contains the lookup code
    ///     foreign key e.g. PatientSexCode
    /// </param>
    /// <param name="lookupDescriptionColumn">The column in the lookup table containing the code description that you want</param>
    /// <param name="lookupTablePrimaryKey">The column in the lookup which contains the code e.g. LocationCode</param>
    /// <param name="collation">Optional - the collation to use when linking the columns</param>
    /// <param name="alsoCreateExtractionInformations">
    ///     True to create a new virtual column in the main dataset so that the code description appears inline with the rest
    ///     of
    ///     the columns in the dataset (when the SELECT query is built)
    /// </param>
    [UseWithObjectConstructor]
    public ExecuteCommandCreateLookup(ICatalogueRepository catalogueRepository,
        ExtractionInformation foreignKeyExtractionInformation, ColumnInfo lookupDescriptionColumn,
        ColumnInfo lookupTablePrimaryKey, string collation, bool alsoCreateExtractionInformations)
        : this(catalogueRepository, foreignKeyExtractionInformation, new[] { lookupDescriptionColumn },
            new List<Tuple<ColumnInfo, ColumnInfo>>
                { Tuple.Create(foreignKeyExtractionInformation.ColumnInfo, lookupTablePrimaryKey) }, collation,
            alsoCreateExtractionInformations)
    {
    }

    /// <inheritdoc />
    public override void Execute()
    {
        base.Execute();

        foreach (var descCol in _lookupDescriptionColumns)
        {
            var lookup = new Lookup(_catalogueRepository, descCol, _fkToPkTuples.First().Item1,
                _fkToPkTuples.First().Item2, ExtractionJoinType.Left, _collation);

            foreach (var supplementalKeyPair in _fkToPkTuples.Skip(1))
                new LookupCompositeJoinInfo(_catalogueRepository, lookup, supplementalKeyPair.Item1,
                    supplementalKeyPair.Item2, _collation);

            if (_alsoCreateExtractionInformations)
            {
                var proposedName = _lookupDescriptionColumns.Length == 1
                    ? $"{_foreignKeyExtractionInformation.GetRuntimeName()}_Desc"
                    : $"{_foreignKeyExtractionInformation.GetRuntimeName()}_{descCol.GetRuntimeName()}";

                var newCatalogueItem = new CatalogueItem(_catalogueRepository, _catalogue, proposedName);
                newCatalogueItem.SetColumnInfo(descCol);

                //bump everyone down 1
                foreach (var toBumpDown in _allExtractionInformations.Where(e =>
                             e.Order > _foreignKeyExtractionInformation.Order))
                {
                    toBumpDown.Order++;
                    toBumpDown.SaveToDatabase();
                }

                var newExtractionInformation =
                    new ExtractionInformation(_catalogueRepository, newCatalogueItem, descCol, descCol.ToString())
                    {
                        ExtractionCategory = ExtractionCategory.Supplemental,
                        Alias = newCatalogueItem.Name,
                        Order = _foreignKeyExtractionInformation.Order + 1
                    };
                newExtractionInformation.SaveToDatabase();
            }
        }

        _catalogue.ClearAllInjections();
    }
}