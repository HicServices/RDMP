// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.CommandExecution;

namespace Rdmp.UI.CommandExecution;

/// <summary>
/// Handles the commencement of drag operations.  This involves deciding whether a given object can be dragged and parceling up the object
/// into an <see cref="ICombineToMakeCommand"/> (which will gather relevant facts about the object).  Dropping is handled by <see cref="ICommandExecutionFactory"/>
/// </summary>
public interface ICombineableFactory
{
    /// <summary>
    /// Creates a new packaged command initiation object from the given Object List View <paramref name="o"/> e.g. for a drag
    /// and drop operation.  The resulting <see cref="ICombineToMakeCommand"/> can be waved around over other objects to test
    /// for a valid command combination
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    ICombineToMakeCommand Create(OLVDataObject o);

    /// <summary>
    /// Creates a new packaged command initiation object from the given dragged object <paramref name="e"/>.  The resulting
    /// <see cref="ICombineToMakeCommand"/> can be waved around over other objects to test for a valid command combination
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    ICombineToMakeCommand Create(ModelDropEventArgs e);

    /// <summary>
    /// Creates a new packaged command initiation object from the given dragged object <paramref name="e"/>.  The resulting
    /// <see cref="ICombineToMakeCommand"/> can be waved around over other objects to test for a valid command combination
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    ICombineToMakeCommand Create(DragEventArgs e);

    /// <summary>
    /// Creates a new packaged command initiation object from the given dragged <paramref name="files"/>.  The resulting
    /// <see cref="ICombineToMakeCommand"/> can be waved around over other objects to test for a valid command combination
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    ICombineToMakeCommand Create(FileInfo[] files);

    /// <summary>
    /// Creates a new packaged command initiation object from the given dragged <paramref name="modelObject"/>.  The resulting
    /// <see cref="ICombineToMakeCommand"/> can be waved around over other objects to test for a valid command combination.
    /// </summary>
    /// <param name="modelObject">A C# object e.g. <see cref="Rdmp.Core.Curation.Data.Catalogue"/> that dragging has begun on</param>
    /// <returns></returns>
    ICombineToMakeCommand Create(object modelObject);

}