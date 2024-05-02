// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.DataExport.DataExtraction.Commands;

/// <summary>
///     Collection of all the datasets and custom tables that are available for extraction.  Since datasets can take a long
///     time to extract sometimes a user
///     will opt to only extract a subset of datasets (or he may have already succesfully extracted some datasets).  This
///     collection should contain a full
///     set of all the things that can be run for a given ExtractionConfiguration (including dataset specific lookups etc).
///     <para>Use ExtractCommandCollectionFactory to create instances of this class.</para>
/// </summary>
public class ExtractCommandCollection
{
    public IExtractDatasetCommand[] Datasets { get; set; }


    public ExtractCommandCollection(IEnumerable<ExtractDatasetCommand> datasetBundles)
    {
        Datasets = datasetBundles.ToArray();
    }
}