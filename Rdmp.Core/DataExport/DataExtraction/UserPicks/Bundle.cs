// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.DataExport.DataExtraction.Commands;

namespace Rdmp.Core.DataExport.DataExtraction.UserPicks;

/// <summary>
///     Ostensibly data extraction is simple: 1 Extraction Configuration, x datasets + 1 cohort.  In practice there are
///     bundled Lookup tables, SupportingDocuments
///     for each dataset, Global SupportingDocuments and even Custom Cohort Data.  The user doesn't nessesarily want to
///     extract everything all the time.
///     Bundles are the collection classes for recording what subset of an ExtractionConfiguration should be run.
/// </summary>
public abstract class Bundle
{
    public Dictionary<object, ExtractCommandState> States { get; }

    public object[] Contents => States.Keys.ToArray();

    protected Bundle(object[] finalObjectsDoNotAddToThisLater)
    {
        //Add states for all objects
        States = new Dictionary<object, ExtractCommandState>();

        foreach (var o in finalObjectsDoNotAddToThisLater)
            States.Add(o, ExtractCommandState.NotLaunched);
    }

    public void SetAllStatesTo(ExtractCommandState state)
    {
        foreach (var k in States.Keys.ToArray())
            States[k] = state;
    }

    public void DropContent(object toDrop)
    {
        //remove the state information
        States.Remove(toDrop);

        //tell child to remove the object too
        OnDropContent(toDrop);
    }

    protected abstract void OnDropContent(object toDrop);
}