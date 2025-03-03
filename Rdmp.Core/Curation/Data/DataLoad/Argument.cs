// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.Data.Remoting;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
/// Abstract base for all concrete IArgument objects.  An Argument is a stored value for a Property defined on a PipelineComponent or DLE component which has
/// been decorated with [DemandsInitialization] and for which the user has picked a value.  The class includes both the Type of the argument (extracted from
/// the class Property PropertyInfo via reflection) and the Value (stored in the database as a string).
/// 
/// <para>This allows simple UI driven population and persistence of configuration settings for plugin and system core components as they are used in all pipeline and
/// dle activities.  See ArgumentCollection for UI logic.</para>
/// </summary>
public abstract class Argument : DatabaseEntity, IArgument
{
    /// <summary>
    /// All Types that are supported for <see cref="Type"/> and <see cref="Value"/> of <see cref="IArgument"/>s.
    /// 
    /// <para>Or to put it another way, don't decorate an <see cref="IArgumentHost"/> property with <see cref="DemandsInitializationAttribute"/> if its Type isn't on this list</para>
    /// </summary>
    public static readonly Type[] PermissableTypes =
    {
        typeof(char?), typeof(char),
        typeof(int?), typeof(int),
        typeof(DateTime?), typeof(DateTime),
        typeof(double?), typeof(double),
        typeof(float?), typeof(float),

        typeof(bool), //no nullable bools please

        typeof(string), typeof(FileInfo),
        typeof(DirectoryInfo),
        typeof(Enum), typeof(Uri), typeof(Regex),

        typeof(Type),

        //IMapsDirectlyToDatabaseTable
        typeof(TableInfo), typeof(ColumnInfo), typeof(PreLoadDiscardedColumn), typeof(LoadProgress),
        typeof(LoadMetadata),
        typeof(CacheProgress), typeof(ExternalDatabaseServer), typeof(StandardRegex),
        typeof(CohortIdentificationConfiguration),
        typeof(RemoteRDMP), typeof(Catalogue), typeof(CatalogueItem),
        typeof(DataAccessCredentials), typeof(RegexRedactionConfiguration),

        //weird special cases
        typeof(ICustomUIDrivenClass), typeof(EncryptedString),

        //special static argument type, always gets the same value never has a database persisted value
        typeof(CatalogueRepository),

        //user must be IDemandToUseAPipeline<T>
        typeof(Pipeline),
        typeof(Datasets.Dataset)
    };

    #region Database Properties

    private string _name;
    private string _value;
    private string _type;
    private string _description;

    /// <inheritdoc/>
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <inheritdoc/>
    [AdjustableLocation]
    public string Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    /// <inheritdoc/>
    public string Type
    {
        get => _type;
        protected set => SetField(ref _type, value);
    }

    /// <inheritdoc/>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    #endregion

    /// <inheritdoc/>
    protected Argument() : base()
    {
    }

    /// <inheritdoc/>
    protected Argument(ICatalogueRepository repository, DbDataReader dataReader)
        : base(repository, dataReader)
    {
    }

    /// <inheritdoc/>
    public object GetValueAsSystemType() => Deserialize(Value, Type);

    private object Deserialize(string value, string type)
    {
        //bool
        if (type.Equals(typeof(bool).ToString()))
            return string.IsNullOrWhiteSpace(value) ? false : (object)Convert.ToBoolean(value);

        if (type.Equals(typeof(Type).ToString()))
            return string.IsNullOrWhiteSpace(value) ? null : (object)MEF.GetType(value);

        if (type.Equals(typeof(CatalogueRepository).ToString()) || type.Equals(typeof(ICatalogueRepository).ToString()))
            return Repository;


        //float?
        if (type.Equals(typeof(float?).ToString()) || type.Equals(typeof(float).ToString()))
            return string.IsNullOrWhiteSpace(value) ? null : float.Parse(value);

        //double?
        if (type.Equals(typeof(double?).ToString()) || type.Equals(typeof(double).ToString()))
            return string.IsNullOrWhiteSpace(value) ? null : double.Parse(value);

        //int?
        if (type.Equals(typeof(int?).ToString()) || type.Equals(typeof(int).ToString()))
            return string.IsNullOrWhiteSpace(value) ? null : int.Parse(value);

        //char?
        if (type.Equals(typeof(char?).ToString()) || type.Equals(typeof(char).ToString()))
            return string.IsNullOrWhiteSpace(value) ? null : char.Parse(value);

        //DateTime?
        if (type.Equals(typeof(DateTime?).ToString()) || type.Equals(typeof(DateTime).ToString()))
            return string.IsNullOrWhiteSpace(value) ? null : DateTime.Parse(value);

        //null
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (type.Equals(typeof(Uri).ToString()))
            return new Uri(value);

        if (type.Equals(typeof(string).ToString()))
            return value;

        if (type.Equals(typeof(FileInfo).ToString()))
            return new FileInfo(value);

        if (type.Equals(typeof(DirectoryInfo).ToString()))
            return new DirectoryInfo(value);

        if (type.Equals(typeof(Regex).ToString()))
            return new Regex(value);

        var concreteType = GetConcreteSystemType(type);

        //try to enum it
        if (typeof(Enum).IsAssignableFrom(concreteType))
            return Enum.Parse(concreteType, value);

        //is it ICustomUIDrivenClass
        if (HandleIfICustomUIDrivenClass(value, concreteType, out var customType))
            return customType;

        if (typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(concreteType))
            try
            {
                return Repository.GetObjectByID(concreteType, Convert.ToInt32(value));
            }
            catch (KeyNotFoundException)
            {
                //object has been deleted
                return null;
            }

        if (typeof(Array).IsAssignableFrom(concreteType))
        {
            var elementType = concreteType.GetElementType();
            var ids = value.Split(',').Select(int.Parse).ToArray();

            if (typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(elementType))
            {
                var genericArray = Repository.GetAllObjectsInIDList(elementType, ids).ToArray();
                var typedArray = Array.CreateInstance(elementType, genericArray.Length);

                for (var i = 0; i < genericArray.Length; i++)
                    typedArray.SetValue(genericArray[i], i);

                return typedArray;
            }
        }

        if (typeof(IDictionary).IsAssignableFrom(concreteType)) return DeserializeDictionary(value, concreteType);

        if (type.Equals(typeof(EncryptedString).ToString()))
            return new EncryptedString(CatalogueRepository) { Value = value };

        return type.Equals(typeof(CultureInfo).ToString())
            ? (object)new CultureInfo(value)
            : throw new NotSupportedException($"Custom arguments cannot be of type {type}");
    }

    private bool HandleIfICustomUIDrivenClass(string value, Type concreteType, out object answer)
    {
        answer = null;


        //if it is data driven
        if (typeof(ICustomUIDrivenClass).IsAssignableFrom(concreteType))
        {
            ICustomUIDrivenClass result;

            try
            {
                var t = MEF.GetType(concreteType.FullName);

                result = (ICustomUIDrivenClass)ObjectConstructor.Construct(t, (ICatalogueRepository)Repository);
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"Failed to create an ICustomUIDrivenClass of type {concreteType.FullName} make sure that you mark your class as public, commit it to the catalogue and mark it with the export ''",
                    e);
            }

            try
            {
                result.RestoreStateFrom(value); //, (CatalogueRepository)Repository);
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"RestoreState failed on your ICustomUIDrivenClass called {concreteType.FullName} the restore value was the string value '{value}'",
                    e);
            }

            answer = result;
            return true;
        }

        //it is not a custom ui driven type
        return false;
    }

    /// <inheritdoc/>
    public Type GetSystemType() => GetSystemType(Type);

    private Type GetSystemType(string type)
    {
        //if we know they type (it is exactly one we are expecting)
        foreach (var knownType in PermissableTypes)
            //return the type
            if (knownType.ToString().Equals(type))
                return knownType;

        var arrayType = new Regex(@"(.*)\[]");
        var arrayMatch = arrayType.Match(type);

        if (arrayMatch.Success)
        {
            var elementTypeAsString = arrayMatch.Groups[1].Value;

            //it is an unknown Type e.g. Bob where Bob is an ICustomUIDrivenClass or something
            var elementType = MEF.GetType(elementTypeAsString) ?? throw new Exception(
                $"Could not figure out what SystemType to use for elementType = '{elementTypeAsString}' of Type '{type}'");
            return Array.CreateInstance(elementType, 0).GetType();
        }

        if (IsDictionary(type, out var kType, out var vType))
        {
            var genericClass = typeof(Dictionary<,>);
            var constructedClass = genericClass.MakeGenericType(kType, vType);
            return constructedClass;
        }

        //it is an unknown Type e.g. Bob where Bob is an ICustomUIDrivenClass or something
        var anyType = MEF.GetType(type) ??
                      throw new Exception($"Could not figure out what SystemType to use for Type = '{type}'");
        return anyType;
    }

    /// <inheritdoc/>
    public Type GetConcreteSystemType() => GetConcreteSystemType(Type);

    /// <inheritdoc cref="GetConcreteSystemType()"/>
    public Type GetConcreteSystemType(string typeAsString)
    {
        var type = GetSystemType(typeAsString);

        //if it is interface e.g. ITableInfo fetch instead the TableInfo object
        if (type.IsInterface && type.Name.StartsWith("I"))
        {
            var candidate = MEF.GetType(type.Name[1..]); // chop the 'I' off

            if (!candidate.IsAbstract)
                return candidate;
        }

        return type;
    }

    /// <inheritdoc/>
    public void SetType(Type t)
    {
        //anything that is a child of a permissible type
        //if (!PermissableTypes.Any(tp => tp.IsAssignableFrom(t)))
        //        throw new NotSupportedException("Type " + t + " is not a permissible type for ProcessTaskArguments");

        Type = t.ToString();
    }

    /// <inheritdoc/>
    public void SetValue(object o)
    {
        Value = Serialize(o, Type);
    }

    private string Serialize(object o, string asType)
    {
        switch (o)
        {
            //anything implementing this interface is permitted
            case ICustomUIDrivenClass @class:
                return @class.SaveStateToString();
            case null:
                return null;
            //We are being asked to store a Type e.g. MyPlugins.MyCustomSQLHacker instead of an instance so easy, we just store the Type as a full name
            case Type _:
                return o.ToString();
        }

        //get the system type
        var type = GetSystemType(asType);

        if (o is string)
            return typeof(IEncryptedString).IsAssignableFrom(type)
                ? new EncryptedString(CatalogueRepository)
                {
                    Value = o.ToString()
                }.Value
                : o.ToString();

        //if it's a nullable type find the underlying Type
        if (type is { IsGenericType: true } && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            type = Nullable.GetUnderlyingType(type);

        //if it's an array
        if (type != null && typeof(Array).IsAssignableFrom(type))
        {
            var arr = (Array)o;
            return typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(type.GetElementType())
                ? string.Join(",", arr.Cast<IMapsDirectlyToDatabaseTable>().Select(m => m.ID))
                : throw new NotSupportedException(
                    $"DemandsInitialization arrays must be of Type IMapsDirectlyToDatabaseTable e.g. TableInfo[].  Supplied Type was {type}");
        }

        if (type != null && typeof(IDictionary).IsAssignableFrom(type))
            return SerializeDictionary((IDictionary)o);

        //if we already have a known type set on us
        if (!string.IsNullOrWhiteSpace(asType))
            //if we are not being passed an Enum
            if (!typeof(Enum).IsAssignableFrom(type))
            {
                //if we have been given an illegal typed object
                if (!PermissableTypes.Contains(o.GetType()))
                    throw new NotSupportedException(
                        $"Type {o.GetType()} is not one of the permissible types for ProcessTaskArgument, argument must be one of: {string.Join(',', PermissableTypes.Select(t => t.ToString()))}");

                //if we are passed something o of differing type to the known requested type then someone is lying to someone!
                if (type?.IsInstanceOfType(o) == false)
                    try
                    {
                        return Convert.ChangeType(o, type).ToString();
                    }
                    catch (Exception)
                    {
                        throw new Exception(
                            $"Cannot set value {o} (of Type {o.GetType().FullName}) to on ProcessTaskArgument because it has an incompatible Type specified ({type.FullName})");
                    }
            }

        return o is IMapsDirectlyToDatabaseTable mapped ? mapped.ID.ToString() : o.ToString();
    }

    #region Dictionary Handling

    private IDictionary DeserializeDictionary(string value, Type type)
    {
        var instance = (IDictionary)Activator.CreateInstance(type);

        using var sr = new StringReader(value);
        var doc = XDocument.Load(sr);
        var dict = doc.Element("dictionary");
        foreach (var xElement in dict.Elements("entry"))
        {
            var kElement = xElement.Element("key");
            var kType = kElement.Attribute("type").Value;
            var kValue = kElement.Attribute("o").Value;

            var keyInstance = Deserialize(kValue, kType);

            var vElement = xElement.Element("value");
            var vType = vElement.Attribute("type").Value;
            var vValue = vElement.Attribute("o").Value;

            var valueInstance = Deserialize(vValue, vType);

            instance.Add(keyInstance, valueInstance);
        }

        return instance;
    }

    private string SerializeDictionary(IDictionary dictionary)
    {
        using var sw = new StringWriter();
        using (var xmlWriter = XmlWriter.Create(sw))
        {
            // Build Xml with xw.

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("dictionary");

            foreach (DictionaryEntry entry in dictionary)
            {
                var keyObject = entry.Key;
                var keyObjectType = keyObject.GetType().ToString();

                var valueObject = entry.Value;
                var valueObjectType =
                    valueObject == null ? typeof(object).ToString() : valueObject.GetType().ToString();

                xmlWriter.WriteStartElement("entry");

                xmlWriter.WriteStartElement("key");
                xmlWriter.WriteAttributeString("type", keyObjectType);
                xmlWriter.WriteAttributeString("o", Serialize(keyObject, keyObjectType));
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("value");
                xmlWriter.WriteAttributeString("type", valueObjectType);
                xmlWriter.WriteAttributeString("o", Serialize(valueObject, valueObjectType));
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

        return sw.ToString();
    }

    //regex to match the two types referenced in the Dictionary
    private static readonly Regex DictionaryType = new(
        @"System\.Collections\.Generic\.Dictionary`2\[(.*),(.*)]"
    );

    private bool IsDictionary(string type, out Type kType, out Type vType)
    {
        kType = null;
        vType = null;


        if (type == null)
            return false;

        var match = DictionaryType.Match(type);

        if (!match.Success)
            return false;

        var kString = match.Groups[1].Value;
        var vString = match.Groups[2].Value;

        kType = GetSystemType(kString);
        vType = GetSystemType(vString);

        return true;
    }

    #endregion
}