// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rdmp.Core.ReusableLibraryCode.Serialization;

/// <summary>
///     Special JsonConvert resolver that allows you to ignore properties.  See
///     https://stackoverflow.com/a/13588192/1037948
/// </summary>
public class IgnorableSerializerContractResolver : DefaultContractResolver
{
    protected readonly Dictionary<Type, HashSet<string>> Ignores;

    public IgnorableSerializerContractResolver()
    {
        Ignores = new Dictionary<Type, HashSet<string>>();
    }

    /// <summary>
    ///     Explicitly ignore the given property(s) for the given type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName">one or more properties to ignore.  Leave empty to ignore the type entirely.</param>
    public void Ignore(Type type, params string[] propertyName)
    {
        // start bucket if DNE
        if (!Ignores.ContainsKey(type)) Ignores[type] = new HashSet<string>();

        foreach (var prop in propertyName) Ignores[type].Add(prop);
    }

    /// <summary>
    ///     Is the given property for the given type ignored?
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    private bool IsIgnored(Type type, string propertyName)
    {
        return Ignores.TryGetValue(type, out var ignore) && (
            // if no properties provided, ignore the type entirely
            ignore.Count == 0 || ignore.Contains(propertyName));
    }

    /// <summary>
    ///     The decision logic goes here
    /// </summary>
    /// <param name="member"></param>
    /// <param name="memberSerialization"></param>
    /// <returns></returns>
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (IsIgnored(property.DeclaringType, property.PropertyName)
            // need to check basetype as well for EF -- @per comment by user576838 - LT: but it can be null, so check that too!
            || (property.DeclaringType?.BaseType != null &&
                IsIgnored(property.DeclaringType.BaseType, property.PropertyName)))
            property.ShouldSerialize = _ => false;

        return property;
    }
}