// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Rdmp.Core.Repositories;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Primary;
using Rdmp.Core.Validation.Constraints.Secondary;
using Rdmp.Core.Validation.Constraints.Secondary.Predictor;

namespace Rdmp.Core.Validation;

/// <summary>
/// The Validator is the main entry point into this API. A client would typically create a Validator instance and then
/// add a number of ItemValidators to it.  Alternatively you can use the static method LoadFromXml.  Ensure you set
/// LocatorForXMLDeserialization.
/// 
/// <para>Generally, there are two phases of interaction with a Validator:</para>
/// 
/// <para>1. Design Time
/// During this phase, the client will instantiate and set up a Validator. The Check() method can be called to check for
/// any type incompatibilities prior to running the actual Validate() method.</para>
/// 
/// <para>2. Run Time
/// The Validation(o) method is called, applying the previously set up validation rules to the supplied domain object (o).</para>
/// 
/// <para>Note: As of this writing the Dictionary-based method is under active development, whereas the Object-based method
/// is not adequately developed or tested . PLEASE USE THE DICTIONARY method FOR NOW!</para>
/// </summary>
[XmlRoot("Validator")]
public class Validator
{
    private object _domainObject;
    private Dictionary<string, object> _domainObjectDictionary;

    /// <summary>
    /// Validation rules can reference objects e.g. StandardRegex.  This static property indicates where to get the available instances available
    /// for selection (the Catalogue database).
    /// </summary>
    public static ICatalogueRepositoryServiceLocator LocatorForXMLDeserialization;

    public List<ItemValidator> ItemValidators { get; set; }


    public Validator()
    {
        ItemValidators = new List<ItemValidator>();
    }

    /// <summary>
    /// Adds an ItemValidator to the Validator, specifying the target property (in the object to be validated) and
    /// the type that we expect this property to have. The type is used later in the Check() method. See below..
    /// </summary>
    /// <param name="itemValidator"></param>
    /// <param name="targetProperty"></param>
    /// <param name="expectedType"></param>
    public void AddItemValidator(ItemValidator itemValidator, string targetProperty, Type expectedType)
    {
        if (ItemValidators.Any(iv => iv.TargetProperty.Equals(targetProperty)))
            throw new ArgumentException(
                "TargetProperty is already targeted by another ItemValidator in the collection");

        itemValidator.TargetProperty = targetProperty;
        itemValidator.ExpectedType = expectedType;

        ItemValidators.Add(itemValidator);
    }

    /// <summary>
    /// Returns the ItemValidator associated with the given key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>An Itemvalidator reference, or null if no match found for the supplied key</returns>
    public ItemValidator GetItemValidator(string key)
    {
        try
        {
            return ItemValidators.SingleOrDefault(iv => iv.TargetProperty.Equals(key));
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    /// <summary>
    /// Removes a given (key) ItemValidator from the collection.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>true if the removal succeeded, false otherwise</returns>
    public bool RemoveItemValidator(string key)
    {
        ItemValidator toRemove;

        try
        {
            toRemove = ItemValidators.First(iv => iv.TargetProperty.Equals(key));
        }
        catch (InvalidOperationException)
        {
            return false;
        }


        return ItemValidators.Remove(toRemove);
    }

    /// <summary>
    /// Validate against the supplied domain object, which takes the form of a generic object.
    /// </summary>
    /// <param name="domainObject"></param>
    public ValidationFailure Validate(object domainObject)
    {
        _domainObject = domainObject;

        return ValidateAgainstDomainObject();
    }

    /// <summary>
    /// Validate against the supplied domain object, which takes the form of a generic object with Properties matching TargetProperty or an SqlDataReader or a DataTable
    /// </summary>
    /// <param name="domainObject"></param>
    /// /// <param name="currentResults">It is expected that Validate is called multiple times (once per row) therefore you can store the Result of the last one and pass it back into the method the next time you call it in order to maintain a running total</param>
    /// <param name="worstConsequence"></param>
    public VerboseValidationResults ValidateVerboseAdditive(object domainObject,
        VerboseValidationResults currentResults, out Consequence? worstConsequence)
    {
        worstConsequence = null;

        _domainObject = domainObject;

        //first time initialize the results by calling its constructor (with all the ivs)
        currentResults ??= new VerboseValidationResults(ItemValidators.ToArray());

        var result = ValidateAgainstDomainObject();

        if (result != null)
            worstConsequence = currentResults.ProcessException(result);

        return currentResults;
    }


    /// <summary>
    /// Validate against the supplied domain object, which takes the form of a Dictionary.
    /// </summary>
    /// <param name="d"></param>
    public ValidationFailure Validate(Dictionary<string, object> d)
    {
        _domainObjectDictionary = d;

        return ValidateAgainstDictionary();
    }

    //This is static because creating new ones with the Type[] causes memory leaks in unmanaged memory   https://blogs.msdn.microsoft.com/tess/2006/02/15/net-memory-leak-xmlserializing-your-way-to-a-memory-leak/
    private static XmlSerializer _serializer;

    /// <summary>
    /// Instantiate a Validator from a (previously saved) XML string.
    /// </summary>
    /// <param name="xml"></param>
    /// <returns>a Validator</returns>
    public static Validator LoadFromXml(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return null;

        InitializeSerializer();

        var rdr = XmlReader.Create(new StringReader(xml));
        return (Validator)_serializer.Deserialize(rdr);
    }

    private static void InitializeSerializer()
    {
        _serializer ??= new XmlSerializer(typeof(Validator), GetExtraTypes().ToArray());
    }

    /// <summary>
    /// Persist the current Validator instance to a string containing XML.
    /// </summary>
    /// <returns>a String</returns>
    public string SaveToXml(bool indent = true)
    {
        var sb = new StringBuilder();

        InitializeSerializer();

        using (var sw = XmlWriter.Create(sb, new XmlWriterSettings { Indent = indent }))
        {
            _serializer.Serialize(sw, this);
        }

        return sb.ToString();
    }

    private static readonly Lock OLockExtraTypes = new();
    private static List<Type> _extraTypes;

    private static List<Type> RefreshExtraTypes()
    {
        lock (OLockExtraTypes)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(
                //type is
                type =>
                    type != null &&
                    //of the correct Type
                    (typeof(IConstraint).IsAssignableFrom(type) ||
                     typeof(PredictionRule).IsAssignableFrom(type)) //Constraint or prediction
                    &&
                    !type.IsAbstract
                    &&
                    !type.IsInterface
                    &&
                    type.IsClass
            ).ToList();
        }
    }

    public static List<Type> GetExtraTypes()
    {
        lock (OLockExtraTypes)
        {
            return _extraTypes ??= RefreshExtraTypes();
        }
    }


    /// <summary>
    /// This Factory method returns a new RegularExpression instance.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static ISecondaryConstraint CreateRegularExpression(string pattern) => new RegularExpression(pattern);

    /// <summary>
    /// Returns an array of available PrimaryConstraint names.
    /// Provides support for client applications who may need to display a list for selection.
    /// </summary>
    /// <returns></returns>
    public static string[] GetPrimaryConstraintNames()
    {
        var primaryConstraintTypes = FindSubClassesOf<PrimaryConstraint>();

        return primaryConstraintTypes.Select(t => t.Name.ToLower()).ToArray();
    }

    public static string[] GetSecondaryConstraintNames()
    {
        var secondaryConstraintTypes = FindSubClassesOf<SecondaryConstraint>().Where(t => t.IsAbstract == false);

        return secondaryConstraintTypes.Select(t => t.Name.ToLower()).ToArray();
    }

    /// <summary>
    /// This Factory method returns a Constraint corresponding to the supplied constraint name.
    /// The name must match the corresponding class name. Matching is case-insensitive.
    /// An ArgumentException is thrown if a matching constraint cannot be found.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="consequence"></param>
    /// <returns>A Constraint</returns>
    public static IConstraint CreateConstraint(string name, Consequence consequence)
    {
        var type = GetExtraTypes().Single(t => t.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        var toReturn = (IConstraint)Activator.CreateInstance(type);
        toReturn.Consequence = consequence;
        return toReturn;
    }

    #region Fluent API experiment

    public ItemValidator ConstrainField(string fieldToValidate, Type type)
    {
        var i = new ItemValidator();
        AddItemValidator(i, fieldToValidate, type);

        return i;
    }

    public ItemValidator EnsureThatDate(string fieldToValidate)
    {
        var i = new ItemValidator();
        AddItemValidator(i, fieldToValidate, typeof(DateTime));

        return i;
    }

    public ItemValidator EnsureThatValue(string fieldToValidate)
    {
        var i = new ItemValidator();
        AddItemValidator(i, fieldToValidate, typeof(double));

        return i;
    }

    #endregion

    #region private methods

    private static IEnumerable<Type> FindSubClassesOf<TBaseType>()
    {
        return GetExtraTypes().Where(t => t.IsSubclassOf(typeof(TBaseType)));
    }

    private ValidationFailure ValidateAgainstDictionary()
    {
        var keys = new string[_domainObjectDictionary.Keys.Count];
        var vals = new object[_domainObjectDictionary.Values.Count];

        _domainObjectDictionary.Keys.CopyTo(keys, 0);
        _domainObjectDictionary.Values.CopyTo(vals, 0);


        var eList = new List<ValidationFailure>();

        //for all the columns we need to validate
        foreach (var itemValidator in ItemValidators)
            if (_domainObjectDictionary.TryGetValue(itemValidator.TargetProperty, out var o))
            {
                //get the first validation failure for the given column (or null if it is valid)
                var result = itemValidator.ValidateAll(o, vals, keys);

                //if it wasn't valid then add it to the eList
                if (result is { SourceItemValidator: null })
                {
                    result.SourceItemValidator = itemValidator;
                    eList.Add(result);
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"Validation failed: Target field [{itemValidator.TargetProperty}] not found in dictionary.");
            }

        return eList.Count > 0 ? new ValidationFailure("There are validation errors.", eList) : null;
    }

    private ValidationFailure ValidateAgainstDomainObject()
    {
        var eList = new List<ValidationFailure>();

        #region work out other column values

        string[] names = null;
        object[] values = null;

        var o = _domainObject;

        switch (o)
        {
            case DbDataReader reader:
                {
                    names = new string[reader.FieldCount];
                    values = new object[reader.FieldCount];

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        names[i] = reader.GetName(i);
                        values[i] = reader[i] == DBNull.Value ? null : reader[i].ToString();
                    }

                    break;
                }
            case DataRow row:
                {
                    names = new string[row.Table.Columns.Count];
                    values = new object[row.Table.Columns.Count];

                    for (var i = 0; i < row.Table.Columns.Count; i++)
                    {
                        names[i] = row.Table.Columns[i].ColumnName;
                        values[i] = row[i] == DBNull.Value ? null : row[i].ToString();
                    }

                    break;
                }
        }

        #endregion


        foreach (var itemValidator in ItemValidators)
        {
            if (itemValidator.TargetProperty == null)
                throw new NullReferenceException("Target property cannot be null");

            try
            {
                ValidationFailure result;

                //see if it has property with this name
                object value;
                switch (o)
                {
                    case DbDataReader reader:
                        {
                            value = reader[itemValidator.TargetProperty];
                            if (value == DBNull.Value)
                                value = null;

                            result = itemValidator.ValidateAll(value, values, names);
                            break;
                        }
                    case DataRow row:
                        result = itemValidator.ValidateAll(row[itemValidator.TargetProperty], values, names);
                        break;
                    default:
                        {
                            var propertiesDictionary = DomainObjectPropertiesToDictionary(o);

                            if (!propertiesDictionary.TryGetValue(itemValidator.TargetProperty, out value))
                                throw new MissingFieldException(
                                    $"Validation failed: Target field [{itemValidator.TargetProperty}] not found in domain object.");

                            result = itemValidator.ValidateAll(value, propertiesDictionary.Values.ToArray(),
                                propertiesDictionary.Keys.ToArray());
                            break;
                        }
                }

                if (result != null)
                {
                    result.SourceItemValidator ??= itemValidator;
                    eList.Add(result);
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException(
                    $"Validation failed: Target field [{itemValidator.TargetProperty}] not found in domain object.");
            }
        }

        return eList.Count > 0 ? new ValidationFailure("There are validation errors.", eList) : null;
    }

    private static Dictionary<string, object> DomainObjectPropertiesToDictionary(object o)
    {
        var toReturn = new Dictionary<string, object>();

        foreach (var prop in o.GetType().GetProperties())
            toReturn.Add(prop.Name, prop.GetValue(o));


        return toReturn;
    }

    #endregion

    public void RenameColumns(Dictionary<string, string> renameDictionary)
    {
        foreach (var kvp in renameDictionary)
            RenameColumn(kvp.Key, kvp.Value);
    }

    public void RenameColumn(string oldName, string newName)
    {
        foreach (var itemValidator in ItemValidators)
        {
            if (itemValidator.TargetProperty == oldName)
                itemValidator.TargetProperty = newName;

            itemValidator.PrimaryConstraint?.RenameColumn(oldName, newName);

            foreach (ISecondaryConstraint constraint in itemValidator.SecondaryConstraints)
                constraint.RenameColumn(oldName, newName);
        }
    }

    public static Type[] GetPredictionExtraTypes()
    {
        return GetExtraTypes().Where(static t => typeof(PredictionRule).IsAssignableFrom(t)).ToArray();
    }
}