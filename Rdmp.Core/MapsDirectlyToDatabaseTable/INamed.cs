// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable;

/// <summary>
/// An IMapsDirectlyToDatabaseTable object who has a property/column called Name which is editable.  Note that you should ensure the ToString property of your
/// class returns the Name field to in order to not drive users crazy.
/// </summary>
public interface INamed : IRevertable
{
    /// <summary>
    /// A user meaningful name for describing the entity, this should be suitable for display in minimal screen space.
    /// </summary>
    [NotNull]
    string Name { get; set; }
}