// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution;

/// <summary>
///     A potentially executable object.  Can be translated into an <see cref="ICommandExecution" /> by an
///     ICommandExecutionFactory.  For example the <see cref="ICombineToMakeCommand" />
///     <see cref="CatalogueCombineable" /> can  be translated into <see cref="ExecuteCommandPutIntoFolder" /> (an
///     ICommandExecution) by combining it with a
///     <see cref="IFolderNode" />.  But you could equally turn it into an
///     <see cref="ExecuteCommandAddCatalogueToCohortIdentificationSetContainer" /> (also an ICommandExecution) by
///     combining it with a CohortAggregateContainer.
///     <para>
///         ICommand should reflect a single object and contain all useful information discovered about the object so that
///         the ICommandExecutionFactory can make a
///         good decision about what ICommandExecution to create as the user drags it about the place.
///     </para>
/// </summary>
public interface ICombineToMakeCommand
{
    /// <summary>
    ///     Returns the Sql (if any) for the object the user is seeking to combine (e.g. by dragging / copying etc).  This
    ///     determines
    ///     what happens when the <see cref="ICombineableSource" /> is combined with an sql endpoint (e.g. SQL text editor in
    ///     the UI).
    /// </summary>
    /// <returns></returns>
    string GetSqlString();
}