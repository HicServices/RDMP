// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;

namespace Rdmp.UI.CommandExecution;

/// <summary>
///     Determines whether drag and drop operations are legal or not.  The legality and action of a given drop is modeled
///     by <see cref="Rdmp.Core.CommandExecution.ICommandExecution" />
/// </summary>
public interface ICommandExecutionFactory
{
    /// <summary>
    ///     Creates an ICommandExecution which reflects the combining of the two objects (<see cref="ICombineToMakeCommand" />
    ///     can even reflect a collection).  If no possible combination of the two objects is possible
    ///     then null is returned.  If the two objects are theoretically usable with one another but the state of the one or
    ///     other is illegal then an ICommandExecution will be returned by the
    ///     IsImpossible/ReasonCommandImpossible flags will be set and it will (should!) crash if run.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="targetModel"></param>
    /// <param name="insertOption"></param>
    /// <returns></returns>
    ICommandExecution Create(ICombineToMakeCommand cmd, object targetModel,
        InsertOption insertOption = InsertOption.Default);

    void Activate(object target);
    bool CanActivate(object target);
}

public enum InsertOption
{
    Default,
    InsertAbove,
    InsertBelow
}