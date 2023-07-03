// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;

namespace Rdmp.UI;

/// <summary>
/// Allows visual studio designer to work with controls which have abstract base classes in the inheritance hierarchy
/// </summary>
/// <typeparam name="TAbstract"></typeparam>
/// <typeparam name="TBase"></typeparam>
public class AbstractControlDescriptionProvider<TAbstract, TBase> : TypeDescriptionProvider
{
    public AbstractControlDescriptionProvider() : base(TypeDescriptor.GetProvider(typeof(TAbstract)))
    {
    }

    public override Type GetReflectionType(Type objectType, object instance)
    {
        return objectType == typeof(TAbstract) ? typeof(TBase) : base.GetReflectionType(objectType, instance);
    }

    public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
    {
        if (objectType == typeof(TAbstract))
            objectType = typeof(TBase);


        return base.CreateInstance(provider, objectType, argTypes, args);
    }
}