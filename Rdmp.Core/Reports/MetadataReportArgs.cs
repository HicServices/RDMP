// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Reports;

/// <summary>
///     Arguments for the <see cref="MetadataReport" /> class which generates a descriptive human readable report about
///     what columns are extracted from the dataset and the
///     data in the underlying tables.
/// </summary>
public class MetadataReportArgs
{
    /// <summary>
    ///     Which datasets to run the metadata report on (all selected Catalogues will appear in the same output file).
    /// </summary>
    public Catalogue[] Catalogues { get; set; }

    /// <summary>
    ///     The length of time to allow queries (e.g. distinct rowcount / graphs) to run for before giving up
    /// </summary>
    public int Timeout { get; set; }

    /// <summary>
    ///     True to generate counts of number of rows in the table under  in the table under the <see cref="Catalogues" />
    /// </summary>
    public bool IncludeRowCounts { get; set; }

    /// <summary>
    ///     True to generate a count of the distinct values in the <see cref="ConcreteColumn.IsExtractionIdentifier" /> column
    ///     (if any) in the table under the <see cref="Catalogues" />
    /// </summary>
    public bool IncludeDistinctIdentifierCounts { get; set; }

    /// <summary>
    ///     True to skip generating any images / graphs etc
    /// </summary>
    public bool SkipImages { get; set; }

    /// <summary>
    ///     The class responsible for figuring out the span of time covered by data in the <see cref="Catalogue" />.  This
    ///     might include throwing out outliers etc.  It may be
    ///     based on the last DQE run on the dataset (therefore could be out of date).
    /// </summary>
    public IDetermineDatasetTimespan TimespanCalculator { get; set; }

    /// <summary>
    ///     True to output data about <see cref="CatalogueItem" /> which are marked
    ///     <see cref="ExtractionCategory.Deprecated" />
    /// </summary>
    public bool IncludeDeprecatedItems { get; set; }

    /// <summary>
    ///     True to output data about <see cref="CatalogueItem" /> which are marked <see cref="ExtractionCategory.Internal" />
    /// </summary>
    public bool IncludeInternalItems { get; set; }

    /// <summary>
    ///     True to output descriptions of <see cref="CatalogueItem" /> even if they don't have any
    ///     <see cref="ExtractionInformation" />
    /// </summary>
    public bool IncludeNonExtractableItems { get; set; }

    /// <summary>
    ///     When outputting <see cref="Lookup" /> tables this is the maximum number of rows that will be written from the
    ///     lookup table.
    /// </summary>
    public int MaxLookupRows { get; set; }

    public MetadataReportArgs(IEnumerable<Catalogue> toReportOn)
    {
        Catalogues = toReportOn.ToArray();
    }
}