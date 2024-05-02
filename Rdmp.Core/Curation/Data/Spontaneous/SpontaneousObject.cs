// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data.Spontaneous;

/// <summary>
///     SpontaneousObjects are 'memory only' versions of IMapsDirectlyToDatabaseTable classes which throw
///     NotSupportedException on any attempt to persist / delete them etc but which
///     you can initialize and set properties on towards your own nefarious ends.
///     <para>
///         E.g. let's say during the course of your programming you want to bolt another container and filter onto an
///         AggregateContainer (in your Catalogue) then you can
///         SpontaneouslyInventedFilterContainer, put the AggregateContainer into it and create a
///         SpontaneouslyInventedFilter along side it.  Then pass the Sponted container
///         to an ISqlQueryBuilder and watch it treat it just like any other normal collection of (database based) filters
///         / containers.
///     </para>
///     <para>
///         SpontaneousObjects all have NEGATIVE IDs which are randomly generated, this lets the RDMP software use ID for
///         object equality without getting confused but prevents the
///         system from ever accidentally saving a SpontaneousObject into a data table in the Catalogue
///     </para>
/// </summary>
public abstract class SpontaneousObject : DatabaseEntity
{
    /// <summary>
    ///     Optional repository for tracking the objects relationship to other <see cref="SpontaneousObject" />
    /// </summary>
    /// <param name="repository"></param>
    protected SpontaneousObject(MemoryRepository repository)
    {
        repository?.InsertAndHydrate(this, new Dictionary<string, object>());
    }

    public override void SaveToDatabase()
    {
    }

    public override void DeleteInDatabase()
    {
        throw new NotSupportedException("Spontaneous objects cannot be deleted");
    }
}