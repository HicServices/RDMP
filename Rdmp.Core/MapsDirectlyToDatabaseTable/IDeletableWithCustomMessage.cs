// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.MapsDirectlyToDatabaseTable;

/// <summary>
///     interface for any <see cref="IDeleteable" /> which wants to describe the unique special snowflake effects of
///     deleting it.  This is primarily intended
///     for classes which are not nouns and instead describe a relationship e.g. SelectedDataSets which describes the fact
///     that dataset X is included in
///     configuration Y.
/// </summary>
public interface IDeletableWithCustomMessage : IDeleteable
{
    /// <summary>
    ///     Verb that describes the effect of deleting this object e.g. 'Remove', 'Disassociate' etc
    /// </summary>
    /// <returns></returns>
    string GetDeleteVerb();

    /// <summary>
    ///     Describes the effects of deleting the IDeletable e.g. Remove dataset X from configuration Y.  Should not be phrased
    ///     as a question.
    /// </summary>
    /// <returns></returns>
    string GetDeleteMessage();
}