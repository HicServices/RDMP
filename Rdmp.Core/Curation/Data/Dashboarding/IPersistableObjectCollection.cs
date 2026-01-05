// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data.Dashboarding;

/// <summary>
/// A collection of database objects used to drive an IObjectCollectionControl which is a user interface tab that requires multiple root objects in order to be created
/// persisted and mainted.  All tabs in RDMP are either IObjectCollectionControl, IRDMPSingleDatabaseObjectControl or RDMPCollectionUI.  Try to avoid using collections if
/// it is possible to hydrate the UI from one database object
/// 
/// <para>A good example of an IObjectCollectionControl (which are driven by IPersistableObjectCollection) is CohortSummaryAggregateGraphUI which requires both a graph and a cohort
/// and the UI shows a summary graph adjusted to match only records in the cohort.</para>
/// </summary>
public interface IPersistableObjectCollection
{
    /// <summary>
    /// A list of all the currently used objects in this collection
    /// </summary>
    List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }

    /// <summary>
    /// Serialize any current state information about the collection that is not encapsulated in <see cref="DatabaseObjects"/> e.g. tickboxes, options, selected enums etc.
    /// <para>Returns null if there is no supplemental information to save about the collection</para>
    /// </summary>
    /// <returns></returns>
    string SaveExtraText();

    /// <summary>
    /// Hydrate the <see cref="IPersistableObjectCollection"/> state with a value that was created by <see cref="SaveExtraText"/>.  This does not include populating <see cref="DatabaseObjects"/>
    /// which happens separately.
    /// </summary>
    /// <param name="s"></param>
    void LoadExtraText(string s);
}