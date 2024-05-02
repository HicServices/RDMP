// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NPOI.XWPF.UserModel;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Reports.ExtractionTime;

/// <summary>
///     Generates tables in a Microsoft Word document describing a Catalogue, its CatalogueItems and any Issues associated
///     with it.  This is used in data extraction
///     to generate metadata documents for the researchers to read (See WordDataWriter)
/// </summary>
public class WordCatalogueExtractor : DocXHelper
{
    private ICatalogue Catalogue { get; }

    //This is an alternative for [DoNotExtractProperty] that only applies to this class where [DoNotExtractProperty] applies to all users of Catalogue e.g. DITAExtractor
    private static readonly string[] PropertyIgnorelist =
    {
        "Statistical_cons", "Research_relevance", "Topic", "Agg_method", "Limitations", "Comments", "Periodicity",
        "Acronym",
        "Detail_Page_URL",
        "Type",
        "Periodicity",
        "Coverage",
        "Number_of_these",
        "BackgroundSummary",
        "Topics",
        "Update_freq",
        "Update_sched",
        "Time_coverage",
        "Last_revision_date",
        "Contact_details",
        "Data_origin",
        "Attribution_citation",
        "Access_options",
        "API_access_URL",
        "Browse_URL",
        "Bulk_Download_URL",
        "Query_tool_URL",
        "Source_URL",
        "Geographical_coverage",
        "Background_summary",
        "Search_keywords",
        "Resource_owner",
        "Granularity",
        "Country_of_origin",
        "Data_standards",
        "Administrative_contact_name",
        "Administrative_contact_email",
        "Administrative_contact_telephone",
        "Administrative_contact_address",
        "Explicit_consent",
        "Ethics_approver",
        "Source_of_data_collection",
        "SubjectNumbers",
        "Ticket",
        "DatasetStartDate",
        "Name"
    };

    private readonly XWPFDocument _document;


    public WordCatalogueExtractor(ICatalogue catalogue, XWPFDocument document)
    {
        Catalogue = catalogue;
        _document = document;
    }

    /// <summary>
    /// </summary>
    /// <param name="cataItems">
    ///     The CatalogueItems you want to write out ( should all belong to the same Catalogue) but may be
    ///     a subset of the whole Catalogue e.g. those columns that were actualy extracted
    /// </param>
    /// <param name="supplementalData">
    ///     A bunch of key value pairs (Tuple actually) that accompany a CatalogueItem and should be
    ///     rammed into the table as it is written out, this could contain information only determined at runtime e.g. not part
    ///     of the Catalogue
    /// </param>
    public void AddMetaDataForColumns(CatalogueItem[] cataItems,
        Dictionary<CatalogueItem, Tuple<string, string>[]> supplementalData = null)
    {
        InsertHeader(_document, $"Catalogue:{Catalogue.Name}");

        //first of all output all the info in the Catalogue

        var requiredRowsCount = CountWriteableProperties(Catalogue);

        var table = InsertTable(_document, requiredRowsCount, 2);

        GenerateObjectPropertiesAsRowUsingReflection(table, Catalogue, null);

        //then move onto CatalogueItems that have been extracted
        foreach (var catalogueItem in cataItems)
        {
            InsertHeader(_document, catalogueItem.Name, 2);

            requiredRowsCount = CountWriteableProperties(catalogueItem);

            //allocate extra space for supplementalData
            if (supplementalData != null)
                requiredRowsCount += supplementalData[catalogueItem].Length;

            //create a new table
            var t = InsertTable(_document, requiredRowsCount, 2);

            if (supplementalData != null && supplementalData.TryGetValue(catalogueItem, out var value))
                GenerateObjectPropertiesAsRowUsingReflection(t, catalogueItem, value);
            else
                GenerateObjectPropertiesAsRowUsingReflection(t, catalogueItem, null);
        }
    }

    private static int CountWriteableProperties(object o)
    {
        var propertyInfo =
            o.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        //generate a row for each property
        return propertyInfo
            .Where(property =>
                property.CanRead && Attribute.IsDefined(property, typeof(DoNotExtractProperty)) == false &&
                !PropertyIgnorelist.Contains(property.Name)).Count(property =>
                property.PropertyType.IsValueType || property.PropertyType.IsEnum ||
                property.PropertyType == typeof(string));
    }

    private static void GenerateObjectPropertiesAsRowUsingReflection(XWPFTable table, object o,
        Tuple<string, string>[] supplementalData)
    {
        var propertyInfo =
            o.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        var currentRow = 0;

        //generate a row for each property
        foreach (var property in propertyInfo)
            //Check whether property can be written to
            if (property.CanRead && Attribute.IsDefined(property, typeof(DoNotExtractProperty)) == false &&
                !PropertyIgnorelist.Contains(property.Name) && (property.PropertyType.IsValueType ||
                                                                property.PropertyType.IsEnum ||
                                                                property.PropertyType == typeof(string)))
            {
                SetTableCell(table, currentRow, 0, property.Name);

                var val = property.GetValue(o, null);

                if (val != null)
                    SetTableCell(table, currentRow, 1, val.ToString());

                currentRow++;
            }

        //add any supplemental data they want in the table
        if (supplementalData != null)
            foreach (var tuple in supplementalData)
            {
                SetTableCell(table, currentRow, 0, tuple.Item1);
                SetTableCell(table, currentRow, 1, tuple.Item2);
                currentRow++;
            }

        //table.AutoFit = AutoFit.Contents;
    }
}