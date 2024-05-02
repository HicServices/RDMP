// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Reflection;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
///     Class for documenting properties declared as [DemandsInitialization] in a class.  Includes the
///     DemandsInitializationAttribute (Description, Mandatory etc) and the
///     PropertyInfo (reflection) of the class as well as the parent propertyinfo if PropertyInfo is defined in a
///     [DemandsNestedInitialization] sub component class of the
///     of the class being evaluated.
/// </summary>
public class RequiredPropertyInfo
{
    /// <summary>
    ///     The attribute that decorates the public property on the class who is demanding that the user provide a value (in an
    ///     <see cref="IArgument" />)
    /// </summary>
    public DemandsInitializationAttribute Demand { get; set; }

    /// <summary>
    ///     The public property on the class who is demanding that the user provide a value (in an <see cref="IArgument" />)
    /// </summary>
    public PropertyInfo PropertyInfo { get; }

    /// <summary>
    ///     Null unless the demand is for a property on a settings class of the main class e.g. MyPlugin has a property
    ///     Settings marked with [DemandsNestedInitialization]
    ///     and this <see cref="RequiredPropertyInfo" /> is for one of the public [DemandsInitialization] decorated properties
    ///     of Settings.  If this is the case then
    ///     <see cref="ParentPropertyInfo" /> will be the root property Settings.
    /// </summary>
    public PropertyInfo ParentPropertyInfo { get; set; }

    /// <summary>
    ///     Records the fact that a given public property on a class is marked with
    ///     <see cref="DemandsInitializationAttribute" /> and that the user is supposed
    ///     to provide a value for it in an <see cref="IArgument" />
    /// </summary>
    /// <param name="demand"></param>
    /// <param name="propertyInfo"></param>
    /// <param name="parentPropertyInfo"></param>
    public RequiredPropertyInfo(DemandsInitializationAttribute demand, PropertyInfo propertyInfo,
        PropertyInfo parentPropertyInfo = null)
    {
        Demand = demand;
        ParentPropertyInfo = parentPropertyInfo;
        PropertyInfo = propertyInfo;
    }

    /// <summary>
    ///     The property name.  If the property is a nested one (i.e. DemandsNestedInitialization) then returns the full
    ///     expression parent.property
    /// </summary>
    public string Name =>
        ParentPropertyInfo == null ? PropertyInfo.Name : $"{ParentPropertyInfo.Name}.{PropertyInfo.Name}";
}