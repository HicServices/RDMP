// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Curation;

/// <summary>
/// Creates a Catalogue from a TableInfo (See TableInfoImporter for how to create a TableInfo from your live database table).  A Catalogue is an extractable dataset
/// which can be made by joining multiple underlying tables and often contains only a subset of columns (those that are extractable to researchers).
/// </summary>
public class ForwardEngineerCatalogue
{
    private readonly ITableInfo _tableInfo;
    private readonly ColumnInfo[] _columnInfos;


    [Obsolete("markAllExtractable is ignored, this constructor is included for API backwards compatibility only.")]
    public ForwardEngineerCatalogue(ITableInfo tableInfo, ColumnInfo[] columnInfos, bool markAllExtractable)
        : this(tableInfo, columnInfos)
    {
    }

    /// <summary>
    /// Sets up the class to create a new <see cref="Catalogue"/> from the supplied table reference
    /// </summary>
    /// <param name="tableInfo"></param>
    /// <param name="columnInfos"></param>
    public ForwardEngineerCatalogue(ITableInfo tableInfo, ColumnInfo[] columnInfos)
    {
        _tableInfo = tableInfo;
        _columnInfos = columnInfos;
    }


    /// <inheritdoc cref="ExecuteForwardEngineering()"/>
    public void ExecuteForwardEngineering(out ICatalogue catalogue, out CatalogueItem[] items,
        out ExtractionInformation[] extractionInformations)
    {
        ExecuteForwardEngineering(null, out catalogue, out items, out extractionInformations);
    }

    /// <summary>
    /// Creates a new <see cref="Catalogue"/> with <see cref="CatalogueItem"/> and <see cref="ExtractionInformation"/> with a one-to-one mapping to
    ///  the <see cref="ColumnInfo"/> this class was constructed with.
    /// </summary>
    public void ExecuteForwardEngineering()
    {
        ExecuteForwardEngineering(null, out _, out _, out _);
    }

    /// <summary>
    /// Creates new <see cref="CatalogueItem"/> and <see cref="ExtractionInformation"/> with a one-to-one mapping to the <see cref="ColumnInfo"/> this class was constructed with.
    /// 
    /// <para>These new columns are added to an existing <see cref="Catalogue"/>.  Use this if you want a dataset that draws data from 2 tables using a <see cref="JoinInfo"/></para>
    /// </summary>
    /// <param name="intoExistingCatalogue"></param>
    public void ExecuteForwardEngineering(ICatalogue intoExistingCatalogue)
    {
        ExecuteForwardEngineering(intoExistingCatalogue, out _, out _, out _);
    }

    /// <inheritdoc cref="ExecuteForwardEngineering()"/>
    public void ExecuteForwardEngineering(ICatalogue intoExistingCatalogue, out ICatalogue catalogue,
        out CatalogueItem[] catalogueItems, out ExtractionInformation[] extractionInformations)
    {
        var repo = _tableInfo.CatalogueRepository;

        //if user did not specify an existing catalogue to supplement
        intoExistingCatalogue ??= new Catalogue(repo, _tableInfo.GetRuntimeName());

        catalogue = intoExistingCatalogue;
        var catalogueItemsCreated = new List<CatalogueItem>();
        var extractionInformationsCreated = new List<ExtractionInformation>();

        var order = 0;

        //for each column we will add a new one to the
        foreach (var col in _columnInfos)
        {
            order++;

            //create it with the same name
            var cataItem = new CatalogueItem(repo, intoExistingCatalogue,
                col.Name[(col.Name.LastIndexOf(".", StringComparison.Ordinal) + 1)..].Trim('[', ']', '`', '"'));
            catalogueItemsCreated.Add(cataItem);

            var newExtractionInfo = new ExtractionInformation(repo, cataItem, col, col.Name)
            {
                Order = order
            };
            newExtractionInfo.SaveToDatabase();
            extractionInformationsCreated.Add(newExtractionInfo);
        }

        extractionInformations = extractionInformationsCreated.ToArray();
        catalogueItems = catalogueItemsCreated.ToArray();
    }
}