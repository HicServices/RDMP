// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.QueryBuilding;

namespace Rdmp.Core.DataExport.DataExtraction.Commands;

/// <summary>
///     See ExtractDatasetCommand
/// </summary>
public interface IExtractDatasetCommand : IExtractCommand
{
    ISelectedDataSets SelectedDataSets { get; }

    IExtractableCohort ExtractableCohort { get; set; }
    ICatalogue Catalogue { get; }
    IExtractionDirectory Directory { get; set; }
    IExtractableDatasetBundle DatasetBundle { get; }
    List<IColumn> ColumnsToExtract { get; set; }

    IProject Project { get; }

    void GenerateQueryBuilder();
    ISqlQueryBuilder QueryBuilder { get; set; }

    ICumulativeExtractionResults CumulativeExtractionResults { get; }
    int TopX { get; set; }

    /// <summary>
    ///     If this is a batch extraction then this is the inclusive start date of the data fetched
    /// </summary>
    DateTime? BatchStart { get; set; }

    /// <summary>
    ///     If this is a batch extraction then this is the exclusive end date of the data fetched
    /// </summary>
    /// <inheritdoc />
    DateTime? BatchEnd { get; set; }

    /// <summary>
    ///     Returns the unique server for running the <see cref="QueryBuilder" /> sql on
    /// </summary>
    /// <returns></returns>
    DiscoveredServer GetDistinctLiveDatabaseServer();
}