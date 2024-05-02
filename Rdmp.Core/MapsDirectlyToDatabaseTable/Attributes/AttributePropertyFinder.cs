// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;

/// <summary>
///     Implementation of <see cref="IAttributePropertyFinder" /> in which a specific Attribute only is looked for.  The
///     Attribute is specified by the generic T
/// </summary>
/// <typeparam name="T">The specific attribute you are looking for e.g. SqlAttribute</typeparam>
public class AttributePropertyFinder<T> : IAttributePropertyFinder where T : Attribute
{
    private readonly Dictionary<Type, HashSet<PropertyInfo>> _properties = new();

    public AttributePropertyFinder(IEnumerable<IMapsDirectlyToDatabaseTable> objects)
    {
        foreach (var type in objects.Select(o => o.GetType()).Distinct())
        {
            var propertyInfos = type.GetProperties();

            foreach (var property in propertyInfos)
                //if property has sql flag
                if (property.GetCustomAttributes(typeof(T), true).Any())
                {
                    if (!_properties.ContainsKey(type))
                        _properties.Add(type, new HashSet<PropertyInfo>());

                    if (!_properties[type].Contains(property))
                        _properties[type].Add(property);
                }
        }
    }

    public AttributePropertyFinder(IMapsDirectlyToDatabaseTable o) : this(new[] { o })
    {
    }

    public IEnumerable<PropertyInfo> GetProperties(IMapsDirectlyToDatabaseTable o)
    {
        return _properties.TryGetValue(o.GetType(), out var properties) ? properties : Array.Empty<PropertyInfo>();
    }

    /// <summary>
    ///     Returns true if the provided object has a property that matches the expected attribute
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public bool ObjectContainsProperty(IMapsDirectlyToDatabaseTable arg)
    {
        return _properties.ContainsKey(arg.GetType());
    }

    public T GetAttribute(PropertyInfo property)
    {
        return (T)property.GetCustomAttributes(typeof(T), true).SingleOrDefault();
    }
}