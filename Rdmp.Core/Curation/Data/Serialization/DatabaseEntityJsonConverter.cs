// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Newtonsoft.Json;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.Serialization;

/// <summary>
///     Handles Serialization of Database Entity classes.  Writing is done by by storing the ID, Type and RepositoryType
///     where the object is stored.  Reading is done by
///     using the IRDMPPlatformRepositoryServiceLocator to fetch the instance out of the database.
///     <para>
///         Also stores the ObjectExport SharingUID if available which will allow deserializing shared objects that might
///         only exist in a local import form i.e. with a different ID
///         (<see cref="ShareManager" />)
///     </para>
/// </summary>
public class DatabaseEntityJsonConverter : JsonConverter
{
    private readonly ShareManager _shareManager;

    /// <summary>
    ///     Creates a new serializer for objects stored in RDMP platform databases (only supports
    ///     <see cref="IMapsDirectlyToDatabaseTable" />)
    /// </summary>
    /// <param name="repositoryLocator"></param>
    public DatabaseEntityJsonConverter(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        _shareManager = new ShareManager(repositoryLocator);
    }

    /// <summary>
    ///     Serializes a <see cref="IMapsDirectlyToDatabaseTable" /> by sharing it with
    ///     <see cref="ShareManager.GetObjectFromPersistenceString" />.  This
    ///     creates a pointer only e.g. "Catalogue 123" and if an <see cref="ObjectExport" /> exists then also the
    ///     <see cref="ObjectExport.SharingUID" />
    ///     so that the JSON can be used in other instances (that have imported the <see cref="ShareDefinition" /> of the
    ///     serialized object)
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("PersistenceString");
        writer.WriteRawValue($"\"{_shareManager.GetPersistenceString((IMapsDirectlyToDatabaseTable)value)}\"");
        writer.WriteEndObject();
    }

    /// <summary>
    ///     Deserializes a persisted <see cref="IMapsDirectlyToDatabaseTable" /> by resolving it as a reference and fetching
    ///     the original
    ///     object using <see cref="ShareManager.GetObjectFromPersistenceString" />.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;

        if (reader.TokenType != JsonToken.StartObject)
            throw new JsonReaderException("Malformed json");

        //instance to populate
        reader.Read();

        if (!reader.Value.Equals("PersistenceString"))
            throw new JsonReaderException("Malformed json, expected single property PersistenceString");

        //read the value
        reader.Read();
        var o = _shareManager.GetObjectFromPersistenceString((string)reader.Value);

        reader.Read();

        //read the end object
        return reader.TokenType != JsonToken.EndObject
            ? throw new JsonReaderException("Did not find EndObject")
            : (object)o;
    }

    /// <summary>
    ///     True if <paramref name="objectType" /> is a <see cref="IMapsDirectlyToDatabaseTable" /> (the only thing this class
    ///     can serialize)
    /// </summary>
    /// <param name="objectType"></param>
    /// <returns></returns>
    public override bool CanConvert(Type objectType)
    {
        return typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(objectType);
    }
}