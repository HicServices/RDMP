// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.DataExport.DataExtraction.UserPicks;

/// <summary>
///     The dataset and all additional content related to that dataset within an ExtractionConfiguration which is about to
///     be extracted.  This includes
///     SupportingDocuments, Lookup tables etc).  This is a mutable class and allows you to 'DropContent' if you do not
///     want to extract given parts (e.g. skip
///     the lookups).
/// </summary>
public class ExtractableDatasetBundle : Bundle, IExtractableDatasetBundle
{
    //The main dataset being extracted
    public IExtractableDataSet DataSet { get; }

    //all the rest of the stuff that goes with the dataset
    public List<SupportingDocument> Documents { get; }
    public List<SupportingSQLTable> SupportingSQL { get; }
    public List<IBundledLookupTable> LookupTables { get; }


    public ExtractableDatasetBundle(IExtractableDataSet dataSet, SupportingDocument[] documents,
        SupportingSQLTable[] supportingSQL, ITableInfo[] lookupTables) :
        base(
            new[] { (object)dataSet }.Union(documents).Union(supportingSQL).Union(lookupTables)
                .ToArray() //pass all the objects to the base class so it can allocate initial States
        )
    {
        DataSet = dataSet;
        Documents = documents.ToList();
        SupportingSQL = supportingSQL.ToList();
        LookupTables = lookupTables.Select(t => new BundledLookupTable(t)).Cast<IBundledLookupTable>().ToList();
    }

    public ExtractableDatasetBundle(IExtractableDataSet dataSet)
        : this(dataSet, Array.Empty<SupportingDocument>(), Array.Empty<SupportingSQLTable>(), Array.Empty<TableInfo>())
    {
    }

    public override string ToString()
    {
        return $"{DataSet} Bundle";
    }

    protected override void OnDropContent(object toDrop)
    {
        switch (toDrop)
        {
            case ExtractableDataSet:
                throw new NotSupportedException(
                    $"Cannot drop {toDrop} from Bundle {this}, you cannot perform an extraction without the dataset component (only documents/lookups etc are optional)");
            case SupportingDocument drop:
                Documents.Remove(drop);
                return;
            case SupportingSQLTable item:
                SupportingSQL.Remove(item);
                return;
            case BundledLookupTable table:
                LookupTables.Remove(table);
                return;
            default:
                throw new NotSupportedException($"Did not know how to drop object of type {toDrop}");
        }
    }
}