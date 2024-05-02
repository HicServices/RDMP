// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;

/// <summary>
///     Enhanced version of c# core class <see cref="PropertyChangedEventArgs" /> which supports recording the old and new
///     values that changed when
///     the property was updated.
/// </summary>
public class PropertyChangedExtendedEventArgs : PropertyChangedEventArgs
{
    public virtual object OldValue { get; private set; }
    public virtual object NewValue { get; private set; }

    public PropertyChangedExtendedEventArgs(string propertyName, object oldValue, object newValue)
        : base(propertyName)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}