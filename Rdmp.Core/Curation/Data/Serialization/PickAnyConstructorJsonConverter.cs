// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.Curation.Data.Serialization;

/// <summary>
/// Supports Json deserialization of objects which don't have default (blank) constructors.  Pass the objects you want to use for constructor
/// arguments to classes you want to deserialize.  This JsonConverter will assert that it CanConvert any object for which it finds no default constructor and
/// a single constructor which is compatible with the constructorObjects (or a subset of them)
/// </summary>
public class PickAnyConstructorJsonConverter : JsonConverter
{
    private readonly object[] _constructorObjects;

    /// <summary>
    /// Creates a JSON deserializer that can use any constructors on the class which match <paramref name="constructorObjects"/>
    /// </summary>
    /// <param name="constructorObjects"></param>
    public PickAnyConstructorJsonConverter(params object[] constructorObjects)
    {
        _constructorObjects = constructorObjects;
    }

    /// <summary>
    /// Cannot write, this class is for deserialization only
    /// </summary>
    public override bool CanWrite => false;

    /// <summary>
    /// Cannot write, throws NotImplementedException
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns a hydrated object from <paramref name="reader"/> by invoking the appropriate constructor identified by <see cref="ObjectConstructor.GetConstructors"/>
    /// which matches the parameters provided to <see cref="PickAnyConstructorJsonConverter"/> when it was constructed.
    /// 
    /// <para>If the <paramref name="objectType"/> is <see cref="IPickAnyConstructorFinishedCallback"/> then <see cref="IPickAnyConstructorFinishedCallback.AfterConstruction"/>
    /// will be called</para>
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var constructor = GetConstructors(objectType).Single();

        var instance = constructor.Key.Invoke(constructor.Value.ToArray());

        serializer.Populate(reader, instance);

        if (instance is IPickAnyConstructorFinishedCallback callback)
            callback.AfterConstruction();

        return instance;
    }

    /// <summary>
    /// Returns true if the <paramref name="objectType"/> is a non value Type with one constructor compatible with the parameters provided to
    /// <see cref="PickAnyConstructorJsonConverter"/> when it was constructed.
    /// </summary>
    /// <param name="objectType"></param>
    /// <returns></returns>
    public override bool CanConvert(Type objectType)
    {
        //we do not handle strings,ints etc!
        if (objectType.IsValueType)
            return false;

        //if there is one compatible constructor
        var constructors = GetConstructors(objectType);

        if (constructors.Count == 0)
            return false;

        return constructors.Count == 1
            ? true
            : throw new ObjectLacksCompatibleConstructorException(
                $"There were {constructors.Count} compatible constructors for the constructorObjects provided");
    }

    private Dictionary<ConstructorInfo, List<object>> GetConstructors(Type objectType) =>
        ObjectConstructor.GetConstructors(objectType, false, false, _constructorObjects);
}