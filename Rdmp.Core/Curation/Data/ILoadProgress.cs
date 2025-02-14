// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Describes the progress of a large iterative load which cannot be completed in a single batch.  Includes start and end dates for what is trying to
/// be loaded as well as how far through that process progress has been made up to date.
/// </summary>
public interface ILoadProgress : INamed, ICheckable
{
    /// <summary>
    /// The date the dataset starts at, this is in dataset time e.g. if you have prescribing records held from 2001-01-01 to present then the <see cref="OriginDate"/> is 2001-01-01
    /// </summary>
    DateTime? OriginDate { get; set; }

    /// <summary>
    /// Records how far through the process of loading data into this dataset.  This is updated at the end of a successful data load for a given date range (E.g. the next 10 days
    /// due to be loaded)
    /// </summary>
    DateTime? DataLoadProgress { get; set; }

    /// <summary>
    /// The data load that this object records progress for.  You can have multiple <see cref="ILoadProgress"/> for a single <see cref="ILoadMetadata"/> (data load) for example you
    /// might have loaded Tayside data into biochemistry up to 2017-01-01 but for Fife you have only loaded data up to 2015-01-01 so far.
    /// </summary>
    int LoadMetadata_ID { get; set; }

    /// <inheritdoc cref="LoadMetadata_ID"/>
    ILoadMetadata LoadMetadata { get; }

    /// <summary>
    /// If the data load involves iteratively loading dat from a date based cache of fetched data then this will be the <see cref="ICacheProgress"/> which is responsible for fetching
    /// and saving the cached data to disk (this occurs separately from the data loading).
    /// </summary>
    ICacheProgress CacheProgress { get; }


    /// <summary>
    /// Do not use, is not respected
    /// </summary>
    [Obsolete("Not respected")]
    bool IsDisabled { get; set; }

    /// <summary>
    /// The number of days to load each time the DLE is run with the <see cref="ILoadProgress"/>
    /// </summary>
    int DefaultNumberOfDaysToLoadEachTime { get; }
}