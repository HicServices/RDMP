using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ProcessTaskArgument")]
    public class ProcessTaskArgument : DatabaseObject, IArgument
    {
        [Key]
        public override int ID { get; set; }

        [Required]
        public int ProcessTask_ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string Value { get; set; }

        [Required]
        public string Type { get; set; }
        public string Description { get; set; }

        [ForeignKey("ProcessTask_ID")]
        public virtual ProcessTask ProcessTask { get; set; }

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

        public object GetValueAsSystemType() => Deserialize(Value, Type);

        public void SaveToDatabase()
        {
            throw new NotImplementedException();
        }

        public void SetType(Type t)
        {
            throw new NotImplementedException();
        }

        public void SetValue(object o)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Name;
        }
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
        typeof(RDMPDbContext),

        //user must be IDemandToUseAPipeline<T>
        typeof(Pipeline)
    };
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
        private static readonly Regex DictionaryType = new(
    @"System\.Collections\.Generic\.Dictionary`2\[(.*),(.*)]"
);

        private object Deserialize(string value, string type)
        {
            //bool
            if (type.Equals(typeof(bool).ToString()))
                return string.IsNullOrWhiteSpace(value) ? false : (object)Convert.ToBoolean(value);

            if (type.Equals(typeof(Type).ToString()))
                return string.IsNullOrWhiteSpace(value) ? null : (object)MEF.GetType(value);

            if (type.Equals(typeof(RDMPDbContext).ToString()) || type.Equals(typeof(RDMPDbContext).ToString()))
                return CatalogueDbContext;


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
                    return CatalogueDbContext.GetObjectByID(concreteType, Convert.ToInt32(value));
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
                    var genericArray = CatalogueDbContext.GetAllObjectsInIDList(elementType, ids).ToArray();
                    var typedArray = Array.CreateInstance(elementType, genericArray.Length);

                    for (var i = 0; i < genericArray.Length; i++)
                        typedArray.SetValue(genericArray[i], i);

                    return typedArray;
                }
            }

            if (typeof(IDictionary).IsAssignableFrom(concreteType)) return DeserializeDictionary(value, concreteType);

            if (type.Equals(typeof(EncryptedString).ToString()))
                return new EncryptedString(CatalogueDbContext) { Value = value };

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

                    result = (ICustomUIDrivenClass)ObjectConstructor.Construct(t, CatalogueDbContext);
                }
                catch (Exception e)
                {
                    throw new Exception(
                        $"Failed to create an ICustomUIDrivenClass of type {concreteType.FullName} make sure that you mark your class as public, commit it to the catalogue and mark it with the export ''",
                        e);
                }

                try
                {
                    result.RestoreStateFrom(value); //, Repository);
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

    }
}
